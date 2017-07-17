using System;
using Batzill.Server.Core.Settings;
using System.Net;
using Batzill.Server.Core.Logging;
using System.Threading.Tasks;
using Batzill.Server.Core.ObjectModel;
using Batzill.Server.Core;
using Batzill.Server.Implementations.HttpClient;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Batzill.Server.Core.SSLBindingHelper;

namespace Batzill.Server
{
    public class HttpClientServer : HttpServer
    {
        private static List<string> AlternateHostNames = new List<string>()
        {
            "127.0.0.1",
            "+",
            "*"
        };

        private HttpListener listener;
        private ISSLBindingHelper sslBindingHelper;

        public HttpClientServer(Logger logger, IOperationFactory operationFactory, TaskFactory taskFactory, HttpServerSettings settings, ISSLBindingHelper sslBindingHelper)
            : base(logger, operationFactory, taskFactory)
        {
            this.listener = new HttpListener();
            this.sslBindingHelper = sslBindingHelper;

            this.ApplySettings(settings);
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
            this.ApplyEndpoints(settings);
            this.ApplyTimeouts(settings);
        }

        protected override HttpContext RecieveRequest()
        {
            HttpListenerContext context = this.listener.GetContext();
            return new HttpClientContext(context.Request, context.Response);
        }

        private bool ApplyEndpoints(HttpServerSettings settings)
        {
            this.logger.Log(EventType.ServerSetup, "Set endpoints passed in settings.");

            string[] endpoints = { settings.Default(HttpServerSettingNames.Endpoint) };
            if (!string.IsNullOrEmpty(settings.Get(HttpServerSettingNames.Endpoint)))
            {
                endpoints = settings.Get(HttpServerSettingNames.Endpoint).Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            }

            this.listener.Prefixes.Clear();
            foreach (string endpoint in endpoints)
            {
                this.logger.Log(EventType.ServerSetup, "Try to add endpoint '{0}'.", endpoint);

                Match match = Regex.Match(endpoint, HttpServerSettingFormats.EndpointFormat, RegexOptions.IgnoreCase);
                if (match.Success && match.Groups.Count > 1)
                {
                    string prefix = match.Groups[1].Value;
                    string protocol = match.Groups[2].Value;
                    string host = match.Groups[3].Value;
                    string port = match.Groups[5].Value ?? ServiceConstants.DefaultPort.ToString();
                    string certThumbprint = match.Groups[9].Value;

                    if (Uri.CheckHostName(host) == UriHostNameType.Dns || HttpClientServer.AlternateHostNames.Contains(host))
                    {
                        if (String.Equals("https", protocol, StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (string.IsNullOrEmpty(certThumbprint))
                            {
                                this.logger.Log(EventType.SettingInvalid, "Skipping endpoint '{0}', protocol 'https' but no cert thumprint provided.", endpoint);
                                continue;
                            }

                            string bindingHost = HttpClientServer.AlternateHostNames.Contains(host) ? this.sslBindingHelper.DefaultEndpointHost  : host;
                            if (!this.sslBindingHelper.TryAddOrUpdateCertBinding(certThumbprint, ServiceConstants.ApplicationId, port, bindingHost))
                            {
                                this.logger.Log(EventType.SettingInvalid, "Skipping endpoint '{0}', unable bind certificate '{1}'.", endpoint, certThumbprint);
                                continue;
                            }
                        }

                        this.listener.Prefixes.Add(prefix);
                        this.logger.Log(EventType.ServerSetup, "Added prefix '{0}'.", prefix);
                    }
                    else
                    {
                        this.logger.Log(EventType.SettingInvalid, "Skipping endpoint '{0}', invalid host '{1}'.", endpoint, host);
                    }
                }
                else
                {
                    this.logger.Log(EventType.SettingInvalid, "Skipping endpoint '{0}', unable to parse.", endpoint);
                }
            }

            if (this.listener.Prefixes.Count == 0)
            {
                this.logger.Log(EventType.SettingInvalid, "Settings file did not contain any valid endpoints!");
                throw new ArgumentException("Settings file did not contain any valid endpoints!");
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
