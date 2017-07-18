using Batzill.Server.Core.Logging;
using Batzill.Server.Core.ObjectModel;
using Batzill.Server.Core.Settings;
using System;
using System.Threading.Tasks;

namespace Batzill.Server.Core
{
    public abstract class HttpServer : ISettingsChangable
    {
        public abstract bool IsRunning
        {
            get;
        }

        private TaskFactory taskfactory;
        private IOperationFactory operationFactory;

        private Task mainTask;
        private int maxConcurrentConnections;
        private bool correctConfigured = false;

        protected Logger logger;
        protected HttpServerSettings settings;

        protected HttpServer(Logger logger, IOperationFactory operationFactory, TaskFactory factory)
        {
            this.logger = logger;
            this.operationFactory = operationFactory;
            this.taskfactory = factory;
        }

        protected abstract void StartInternal();
        protected abstract void StopInternal();
        protected abstract void ApplySettingsInternal(HttpServerSettings settings);

        protected abstract HttpContext RecieveRequest();

        public bool Restart()
        {
            this.logger.Log(EventType.SystemInformation, "Attempting to restart the HttpClientServer.");

            return this.Stop() && this.Start();
        }

        public bool Start()
        {
            if (this.correctConfigured)
            {
                try
                {
                    this.logger.Log(EventType.SystemInformation, "Attempting to start the HttpServer.");

                    this.logger.Log(EventType.SystemInformation, "Setp 1: Load operations.");
                    this.operationFactory.LoadOperations();

                    this.logger.Log(EventType.SystemInformation, "Setp 2: Start listening on bindings.");
                    this.StartInternal();

                    this.logger.Log(EventType.SystemInformation, "Setp 3: Start request management.");
                    this.Run();

                    this.logger.Log(EventType.SystemInformation, "Successfully started the HttpServer.");

                    return true;
                }
                catch (Exception ex)
                {
                    this.logger.Log(EventType.SystemError, "Unable to start HttpServer: {0}", ex.ToString());
                    return false;
                }
            }
            else
            {
                this.logger.Log(EventType.SystemInformation, "Settings are in an invalid state, update settings and try again.");
                return false;
            }
        }

        public bool Stop()
        {
            try
            {
                this.logger.Log(EventType.SystemInformation, "Attempting to stop the HttpServer.");

                this.StopInternal();
                
                this.logger.Log(EventType.SystemInformation, "Successfully stopped the HttpServer.");

                return true;
            }
            catch (Exception ex)
            {
                this.logger.Log(EventType.SystemError, "Unable to stop HttpServer: {0}", ex.ToString());
                return false;
            }
        }

        public bool ApplySettings(HttpServerSettings settings)
        {
            this.logger.Log(EventType.ServerSetup, "Attempting to update server settings.");

            if (settings == null)
            {
                this.logger.Log(EventType.ServerSetup, "Provided settings are null, skip update.");
                return false;
            }

            bool startStopServer = this.IsRunning;
            this.settings = settings.Clone();

            if (startStopServer)
            {
                this.logger.Log(EventType.ServerSetup, "HttpServer is running, attempting to stop the gateway for the settings update.");

                if (!this.Stop())
                {
                    this.logger.Log(EventType.ServerSetup, "HttpServer is still running, applying settings failed.");
                    return false;
                }
            }

            try
            {
                // update shared settings first
                this.ApplyCoreSettings(settings);

                // call update method of child 
                this.ApplySettingsInternal(settings);

                this.correctConfigured = true;
            }
            catch(Exception ex)
            {
                this.logger.Log(EventType.SystemError, ex.ToString());
                this.logger.Log(EventType.SystemError, "Error occured while applying settings, please check logs for more information.");
                this.correctConfigured = false;
            }

            if (this.correctConfigured && startStopServer)
            {
                this.logger.Log(EventType.ServerSetup, "HttpServer was running before, attempting to start the gateway after the settings update.");
                if (!this.Start())
                {
                    this.logger.Log(EventType.ServerSetup, "Unable sto start server, applying settings failed.");
                    return false;
                }
            }
            else
            {
                this.logger.Log(EventType.ServerSetup, "HttpServer is in stopped state.");
            }

            return true;
        }

        private void ApplyCoreSettings(HttpServerSettings settings)
        {
            if (!Int32.TryParse(settings.Get(HttpServerSettingNames.ConnectionLimit), out this.maxConcurrentConnections))
            {
                this.maxConcurrentConnections = Convert.ToInt32(settings.Default(HttpServerSettingNames.ConnectionLimit));
            }

            this.logger.Log(EventType.ServerSetup, "Set ConnectionLimit to {0}.", this.maxConcurrentConnections);
        }

        protected void Run()
        {
            this.mainTask = Task.Run(() =>
                {
                    while (this.IsRunning)
                    {
                        try
                        {
                            this.ManageNextRequest();
                        }
                        catch (Exception ex)
                        {
                            this.logger.Log(EventType.SystemError, "Error while working on an incoming request! {0}", ex.Message);
                        }
                    }
                });
        }

        private void ManageNextRequest()
        {
            this.logger.Log(EventType.SystemInformation, "Wait for next request.");

            HttpContext context = this.RecieveRequest();

            this.taskfactory.StartNew(() =>
            {
                string operationId = Guid.NewGuid().ToString();

                this.logger.Log(
                    EventType.SystemInformation, 
                    "Recieved new request {0} {1} HTTP{2}/{3} => id: {4}", 
                    context.Request.HttpMethod, 
                    context.Request.RawUrl, 
                    (context.Request.IsSecureConnection ? "s" : ""), 
                    context.Request.ProtocolVersion,
                    operationId);

                this.ProcessRequest(operationId, context);

                this.logger.Log(EventType.SystemInformation, "Do a final header sync, stream flush and a close.");

                if (context.SyncAllowed)
                {
                    context.SyncResponse();
                }

                if (context.FlushAllowed)
                {
                    context.FlushResponse();
                }

                context.CloseResponse();
                
                this.logger.Log(EventType.SystemInformation, "Operation '{0}' finished successful.", operationId);
            });
        }

        private void ProcessRequest(string operationId, HttpContext context)
        {
            this.logger.Log(EventType.ServerSetup, "Find matching operation for op");

            // create matching operation
            Operation operation = this.operationFactory.CreateMatchingOperation(context);

            // initialize operation
            FrontendOperationLogger logger = new FrontendOperationLogger(this.logger.logWriter, operationId, operation.Name);
            operation.Initialize(logger, settings.Clone(), operationId);

            logger.Log(EventType.OperationInformation, "Start executing operation: '{0}', id: '{1}'.", operation.Name, operation.ID);

            DateTime startTime = DateTime.Now;

            operation.Execute(context);

            this.logger.Log(EventType.ServerSetup, "Successfully finished operation '{0}' with id '{1}' after {2}s.", operation.Name, operation.ID, (DateTime.Now - startTime).TotalSeconds);
        }
    }
}
