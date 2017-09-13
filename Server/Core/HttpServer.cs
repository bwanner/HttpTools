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

        protected HttpServer(Logger logger, IOperationFactory operationFactory)
        {
            this.logger = logger;
            this.operationFactory = operationFactory;
        }

        protected abstract void StartInternal();
        protected abstract void StopInternal();
        protected abstract void ApplySettingsInternal(HttpServerSettings settings);

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

                    this.logger.Log(EventType.SystemInformation, "Step 1: Load operations.");
                    this.operationFactory.LoadOperations();

                    this.logger.Log(EventType.SystemInformation, "Step 2: Start internal server.");
                    this.StartInternal();

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

        protected void HandleRequest(HttpContext context)
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

            try
            {
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
            }
            catch (Exception ex)
            {
                this.logger.Log(EventType.SystemError, "Error executing operation '{0}': {1}", operationId, ex.Message);
            }
        }

        private void ProcessRequest(string operationId, HttpContext context)
        {
            this.logger.Log(EventType.OperationLoading, "Find matching operation for request");

            // create matching operation
            Operation operation = this.operationFactory.CreateMatchingOperation(context);

            // initialize operation
            OperationLogger logger = new OperationLogger(this.logger.logWriter, context.Request.RemoteEndpoint.Address.ToString(), operationId, operation.Name);
            operation.Initialize(logger, settings.Clone(), operationId);

            logger.Log(
                EventType.OperationLoading, 
                "Start executing operation for client '{0}: name: '{1}', id: '{2}', request: '{3}'", 
                context.Request.RemoteEndpoint.Address, 
                operation.Name, 
                operation.ID, 
                context.Request.RawUrl);

            DateTime startTime = DateTime.Now;

            operation.Execute(context);

            this.logger.Log(EventType.OperationLoading, "Successfully finished operation '{0}' with id '{1}' after {2}s.", operation.Name, operation.ID, (DateTime.Now - startTime).TotalSeconds);
        }
    }
}
