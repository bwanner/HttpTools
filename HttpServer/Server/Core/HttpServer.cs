using Batzill.Server.Core.Logging;
using Batzill.Server.Core.ObjectModel;
using Batzill.Server.Core.Settings;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Batzill.Server.Core
{
    public abstract class HttpServer
    {
        public abstract bool IsRunning
        {
            get;
        }

        private TaskFactory taskfactory;
        private IOperationFactory operationFactory;

        private Task mainTask;
        private int maxConcurrentConnections;

        protected Logger logger;
        protected HttpServerSettings settings;

        protected HttpServer(Logger logger, HttpServerSettings settings, IOperationFactory operationFactory, TaskFactory factory)
        {
            this.logger = logger;
            this.operationFactory = operationFactory;
            this.taskfactory = factory;

            settings.Provider.SettingsChanged += this.ApplySettings;
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

        protected void ApplySettings(object sender, HttpServerSettings settings)
        {
            this.logger.Log(EventType.ServerSetup, "Settings update got requested.");

            this.settings = settings.Clone();

            bool successfulSoFar = true;
            bool startStopServer = this.IsRunning;

            if (startStopServer)
            {
                this.logger.Log(EventType.ServerSetup, "HttpServer is running, attempting to stop the gateway for the settings update.");
                this.StopInternal();
            }

            // update shared settings first
            this.ApplyCoreSettings(settings);

            // call update method of child 
            this.ApplySettingsInternal(settings);

            if (successfulSoFar && startStopServer)
            {
                this.logger.Log(EventType.ServerSetup, "HttpServer was running before, attempting to start the gateway after the settings update.");
                this.StartInternal();
            }
        }

        private bool ApplyCoreSettings(HttpServerSettings settings)
        {
            if (!Int32.TryParse(settings.Get(HttpServerSettingNames.ConnectionLimit), out this.maxConcurrentConnections))
            {
                this.maxConcurrentConnections = Convert.ToInt32(settings.Default(HttpServerSettingNames.ConnectionLimit));
            }

            this.logger.Log(EventType.ServerSetup, "Set ConnectionLimit to {0}.", this.maxConcurrentConnections);

            return true;
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

                this.logger.Log(EventType.SystemInformation, "Do a final sync of properties and stream flush.");

                context.SyncResponse();
                context.FlushResponse();
                
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
            operation.Initialize(logger, settings, operationId);

            logger.Log(EventType.OperationInformation, "Start executing operation: '{0}', id: '{1}'.", operation.Name, operation.ID);

            DateTime startTime = DateTime.Now;

            operation.Execute(context);

            this.logger.Log(EventType.ServerSetup, "Successfully finished operation '{0}' with id '{1}' after {2}s.", operation.Name, operation.ID, (DateTime.Now - startTime).TotalSeconds);
        }
    }
}
