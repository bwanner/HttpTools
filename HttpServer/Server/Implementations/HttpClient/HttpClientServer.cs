using System;
using Batzill.Server.Core.Settings;
using System.Net;
using Batzill.Server.Core.Logging;
using System.Threading;
using System.Threading.Tasks;
using Batzill.Server.Core.ObjectModel;
using System.IO;
using Batzill.Server.Core;
using Batzill.Server.Implementations.HttpClient;

namespace Batzill.Server
{
    public class HttpClientServer : HttpServer
    {
        private HttpListener listener;

        public HttpClientServer(Logger logger, HttpServerSettings settings, IOperationFactory operationFactory, TaskFactory taskFactory) 
            : base(logger, settings, operationFactory, taskFactory)
        {
            this.listener = new HttpListener();

            this.ApplySettings(null, settings);
        }

        public override bool IsRunning
        {
            get
            {
                return this.listener.IsListening;
            }
        }

        protected override void StartInternal()
        {
            this.listener.Start();
        }

        protected override void StopInternal()
        {
            this.listener.Stop();
        }

        protected override void ApplySettingsInternal(HttpServerSettings settings)
        {
            this.ApplyPrefixes(settings);
            this.ApplyTimeouts(settings);
        }

        protected override HttpContext RecieveRequest()
        {
            HttpListenerContext context = this.listener.GetContext();
            return new HttpClientContext(context.Request, context.Response);
        }

        private bool ApplyPrefixes(HttpServerSettings settings)
        {
            if (!Int32.TryParse(settings.Get(HttpServerSettingNames.Port), out int port))
            {
                port = Int32.Parse(settings.Default(HttpServerSettingNames.Port));
            }

            string[] protocols = { settings.Default(HttpServerSettingNames.Protocol) };
            if (!string.IsNullOrEmpty(settings.Get(HttpServerSettingNames.Protocol)))
            {
                protocols = settings.Get(HttpServerSettingNames.Protocol).Split(ServiceConstants.ListValueSplitter);
            }

            string[] bindings = { settings.Default(HttpServerSettingNames.Binding) };
            if (!string.IsNullOrEmpty(settings.Get(HttpServerSettingNames.Binding)))
            {
                bindings = settings.Get(HttpServerSettingNames.Binding).Split(ServiceConstants.ListValueSplitter);
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
            if (!Int32.TryParse(settings.Get(HttpServerSettingNames.IdleTimeout), out int idleTimeout))
            {
                idleTimeout = Int32.Parse(settings.Default(HttpServerSettingNames.IdleTimeout));
            }
            this.listener.TimeoutManager.IdleConnection = new TimeSpan(0, 0, idleTimeout);

            this.logger.Log(EventType.ServerSetup, "Set idle timeout to {0}s.", idleTimeout);

            return true;
        }
    }
}
