using System;
using HttpServer.Core.Settings;
using System.Net;
using HttpServer.Core.Logging;

namespace HttpServer.Core
{
    public class HttpClientServer : HttpServerCore
    {
        private const string UniversalBinding = "+";

        private HttpListener listener;

        public HttpClientServer(Logger logger) : base(logger)
        {
            this.listener = new HttpListener();
        }

        public override bool IsRunning
        {
            get
            {
                return this.listener.IsListening;
            }
        }

        public override bool Restart()
        {
            this.logger.Log(EventType.SystemInformation, "Attempting to restart the HttpClientServer.");

            return this.Stop() && this.Start();
        }

        public override bool Start()
        {
            try
            {
                this.logger.Log(EventType.SystemInformation, "Attempting to start the HttpClientServer.");

                this.listener.Start();

                this.logger.Log(EventType.SystemInformation, "Successfully started the HttpClientServer.");

                return true;
            }
            catch (Exception ex)
            {
                this.logger.Log(EventType.SystemError, "Unable to start HttpClientServer: {0}", ex.ToString());
                return false;
            }
        }

        public override bool Stop()
        {
            this.logger.Log(EventType.SystemInformation, "Attempting to stop the HttpClientServer.");

            this.listener.Stop();

            this.logger.Log(EventType.SystemInformation, "Successfully stopped the HttpClientServer.");

            return true;
        }

        protected override bool ApplySettingsInternal(HttpServerSettings settings)
        {
            bool success = true;

            success &= this.ApplyPrefixes(settings);
            success &= this.ApplyTimeouts(settings);

            return success;
        }

        private bool ApplyPrefixes(HttpServerSettings settings)
        {
            if (!Int32.TryParse(settings.Get(HttpServerSettingConstants.Port), out int port))
            {
                port = ServiceConstants.DefaultPort;
            }

            string[] protocols = { ServiceConstants.DefaultProtocol };
            if (!string.IsNullOrEmpty(settings.Get(HttpServerSettingConstants.Protocol)))
            {
                protocols = settings.Get(HttpServerSettingConstants.Protocol).Split(HttpServerSettingConstants.ListValueSplitter);
            }

            string[] bindings = { HttpClientServer.UniversalBinding };
            if (!string.IsNullOrEmpty(settings.Get(HttpServerSettingConstants.Binding)))
            {
                bindings = settings.Get(HttpServerSettingConstants.Binding).Split(HttpServerSettingConstants.ListValueSplitter);
            }

            this.listener.Prefixes.Clear();
            foreach (string binding in bindings)
            {
                foreach (string protocol in protocols)
                {
                    string prefix = string.Format("{0}://{1}:{2}/", protocol.ToLowerInvariant(), binding, port);
                    this.listener.Prefixes.Add(prefix);

                    this.logger.Log(EventType.ServerSetup, "Added binding '{0}'.", prefix);
                }
            }

            return true;
        }

        private bool ApplyTimeouts(HttpServerSettings settings)
        {
            if (!Int32.TryParse(settings.Get(HttpServerSettingConstants.IdleTimeout), out int idleTimeout))
            {
                idleTimeout = ServiceConstants.DefaultIdleTimeout;
            }
            this.listener.TimeoutManager.IdleConnection = new TimeSpan(0, 0, idleTimeout);

            this.logger.Log(EventType.ServerSetup, "Set idle timeout to {0}s.", idleTimeout);

            return true;
        }
    }
}
