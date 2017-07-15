using HttpServer.Core.Logging;
using HttpServer.Core.ObjectModel;
using HttpServer.Core.Settings;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer.Core
{
    public abstract class HttpServerCore
    {
        public abstract bool IsRunning
        {
            get;
        }
        
        protected Logger logger;

        protected int maxConcurrentConnections = ServiceConstants.DefaultMaxConcurrentConnections;

        protected HttpServerCore(Logger logger)
        {
            this.logger = logger;
        }

        public abstract bool Start();
        public abstract bool Restart();
        public abstract bool Stop();
        protected abstract bool ApplySettingsInternal(HttpServerSettings settings);

        public bool ApplySettings(HttpServerSettings settings)
        {
            this.logger.Log(EventType.ServerSetup, "Settings update got requested.");

            bool successfulSoFar = true;
            bool startStopServer = this.IsRunning;

            if (startStopServer)
            {
                this.logger.Log(EventType.ServerSetup, "HttpServer is running, attempting to stop the gateway for the settings update.");
                successfulSoFar &= this.Stop();
            }

            // update shared settings first
            successfulSoFar &= this.ApplyLimits(settings);

            // call update method of child 
            successfulSoFar &= this.ApplySettingsInternal(settings);

            if (successfulSoFar && startStopServer)
            {
                this.logger.Log(EventType.ServerSetup, "HttpServer was running before, attempting to start the gateway after the settings update.");
                successfulSoFar &= this.Start();
            }

            return successfulSoFar;
        }

        private bool ApplyLimits(HttpServerSettings settings)
        {
            if (!Int32.TryParse(settings.Get(HttpServerSettingConstants.ConnectionLimit), out this.maxConcurrentConnections))
            {
                this.maxConcurrentConnections = ServiceConstants.DefaultMaxConcurrentConnections;
            }

            this.logger.Log(EventType.ServerSetup, "Set ConnectionLimit to {0}.", this.maxConcurrentConnections);

            return true;
        }

        protected void HandleRequestSync(HttpRequest request, HttpResponse response)
        {
        }
    }
}
