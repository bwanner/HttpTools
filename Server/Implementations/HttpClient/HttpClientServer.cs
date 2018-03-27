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
using System.Threading;

namespace Batzill.Server
{
    public class HttpClientServer : HttpServer
    {
        private static int InitialConcurrentConnections = 1;

        private static List<string> AlternateHostNames = new List<string>()
        {
            "127.0.0.1",
            "+",
            "*"
        };

        private HttpListener listener;
        private ISSLBindingHelper sslBindingHelper;

        private Semaphore semaphore;

        private bool httpKeepAlive;

        public HttpClientServer(Logger logger, IOperationFactory operationFactory, HttpServerSettings settings, ISSLBindingHelper sslBindingHelper)
            : base(logger, operationFactory)
        {
            this.listener = new HttpListener();
            this.sslBindingHelper = sslBindingHelper;
            this.semaphore = new Semaphore(HttpClientServer.InitialConcurrentConnections, HttpClientServer.InitialConcurrentConnections);

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

            Task.Run(() =>
            {
                while(this.IsRunning)
                {
                    this.semaphore.WaitOne();
                    this.listener.BeginGetContext(new AsyncCallback(ListenerCallback), this.listener);
                }
            });
        }

        private void ListenerCallback(IAsyncResult result)
        {
            HttpListener listener = (HttpListener)result.AsyncState;
            HttpListenerContext httpListenerContext = (result.AsyncState as HttpListener).EndGetContext(result);

            // realease semaphore after ending getcontext operation and before proceeding with request
            this.semaphore.Release();

            // Prepare Context
            HttpClientContext context = new HttpClientContext(httpListenerContext.Request, httpListenerContext.Response);
            context.Response.KeepAlive = this.httpKeepAlive;

            this.HandleRequest(context);
        }

        protected override void StopInternal()
        {
            this.listener.Stop();
            this.semaphore.Close();
        }

        protected override void ApplySettingsInternal(HttpServerSettings settings)
        {
            this.ApplyEndpoints(settings);
            this.ApplyLimits(settings);
        }


        private void ApplyEndpoints(HttpServerSettings settings)
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
                                this.logger.Log(EventType.SettingInvalid, "Skipping endpoint '{0}', unable to bind certificate '{1}'.", endpoint, certThumbprint);
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
        }

        private void ApplyLimits(HttpServerSettings settings)
        {
            /* IdleTimeout */
            if (!Int32.TryParse(settings.Get(HttpServerSettingNames.IdleTimeout), out int idleTimeout))
            {
                this.logger.Log(EventType.SettingsParsingError, "Unable to parse IdleTimeout '{0}'.", settings.Get(HttpServerSettingNames.IdleTimeout));
                throw new ArgumentException("IdleTimeout");
            }
            this.listener.TimeoutManager.IdleConnection = new TimeSpan(0, 0, idleTimeout);

            this.logger.Log(EventType.ServerSetup, "Set idle timeout to {0}s.", idleTimeout);

            /* ConnectionLimit */
            if (!Int32.TryParse(settings.Get(HttpServerSettingNames.ConnectionLimit), out int maxConcurrentConnections))
            {
                this.logger.Log(EventType.SettingsParsingError, "Unable to parse ConnectionLimit '{0}'.", settings.Get(HttpServerSettingNames.ConnectionLimit));
                throw new ArgumentException("ConnectionLimit");
            }
            // server got stopped before settings update, save to reinitialize the semaphore.
            this.semaphore = new Semaphore(maxConcurrentConnections, maxConcurrentConnections);

            this.logger.Log(EventType.ServerSetup, "Set ConnectionLimit to '{0}'.", maxConcurrentConnections);

            /* HttpKeepAlive */
            if (!bool.TryParse(settings.Get(HttpServerSettingNames.HttpKeepAlive), out bool httpKeepAlive))
            {
                this.logger.Log(EventType.SettingsParsingError, "Unable to parse HttpKeepAlive '{0}'.", settings.Get(HttpServerSettingNames.HttpKeepAlive));
                throw new ArgumentException("HttpKeepAlive");
            }
            this.httpKeepAlive = httpKeepAlive;

            this.logger.Log(EventType.ServerSetup, "Set HttpKeepAlive to '{0}'.", httpKeepAlive);
        }
    }
}
