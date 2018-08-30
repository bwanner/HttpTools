using System;
using Batzill.Server.Core.ObjectModel;
using Batzill.Server.Core.Settings;
using System.Text.RegularExpressions;
using Batzill.Server.Core.Settings.Custom.Operations;
using Batzill.Server.Core.Authentication;
using System.Collections.Concurrent;
using Batzill.Server.Core.Logging;

namespace Batzill.Server.Core
{
    public abstract class AuthenticationRequiredOperation : Operation
    {
        public const string AccessTokenName = "AccessToken";

        private static ConcurrentDictionary<Type, bool> AuthenticationEnabled = new ConcurrentDictionary<Type, bool>();

        public AuthenticationRequiredOperation(Logger logger = null) : base(logger)
        {
        }

        public override void InitializeClass(OperationSettings settings, IAuthenticationManager authManager)
        {
            if (!(settings is AuthenticationRequiredOperationSettings))
            {
                throw new ArgumentException($"Type '{settings.GetType()}' is invalid for this operation.");
            }

            AuthenticationRequiredOperation.AuthenticationEnabled[this.GetType()] = (settings as AuthenticationRequiredOperationSettings).AuthenticationRequired;
        }

        protected sealed override void ExecuteInternal(HttpContext context, IAuthenticationManager authManager)
        {
            bool authEnabled = AuthenticationRequiredOperation.AuthenticationEnabled[this.GetType()];

            this.logger?.Log(EventType.OperationAuthentication, "Authentication {0}.", authEnabled ? "ENABLED" : "DISABLED");

            if (authEnabled)
            {
                // Check for https

                if (authManager.HttpsOnly && !context.Request.IsSecureConnection)
                {
                    this.logger?.Log(EventType.OperationAuthenticationError, "HttpsOnly is set but connection to client is 'http'.");

                    context.Response.SetDefaultValues();
                    context.Response.StatusCode = 403;
                    context.Response.WriteContent("Authentication and operations that require authentication are only enabled for https.");

                    return;
                }

                // First check in cookies

                string accessToken = context.Request.Cookies[AuthenticationRequiredOperation.AccessTokenName]?.Value;

                // Check in headers

                if (string.IsNullOrEmpty(accessToken))
                {
                    accessToken = context.Request.Headers[AuthenticationRequiredOperation.AccessTokenName];
                }

                // check in query
                if (string.IsNullOrEmpty(accessToken))
                {
                    var parameters = System.Web.HttpUtility.ParseQueryString(System.Web.HttpUtility.UrlDecode(context.Request.Url.Query));
                    accessToken = parameters[AuthenticationRequiredOperation.AccessTokenName];
                }
                
                this.logger?.Log(EventType.OperationAuthentication, "Found accesstoken: {0}.", string.IsNullOrEmpty(accessToken) ? "false" : "true");

                if (!authManager.IsValidAccessToken(accessToken))
                {
                    this.logger?.Log(EventType.OperationAuthenticationError, "No valid access token passed.");

                    context.Response.SetDefaultValues();
                    context.Response.StatusCode = 403;
                    context.Response.WriteContent("Authentication required! Please authenticate at '/auth?username={USERNAME}&key={KEY}'.");

                    return;
                }

                this.logger?.Log(EventType.OperationAuthentication, "Authentication was successful.");
            }

            this.ExecuteAfterAuthentication(context, authManager);
        }

        protected abstract void ExecuteAfterAuthentication(HttpContext context, IAuthenticationManager authManager);

        public override bool Match(HttpContext context)
        {
            return Regex.IsMatch(context.Request.RawUrl, "^/id$", RegexOptions.IgnoreCase);
        }
    }
}
