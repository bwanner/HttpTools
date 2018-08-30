using System;
using Batzill.Server.Core.Logging;
using Batzill.Server.Core.ObjectModel;
using Batzill.Server.Core.Settings;
using System.Text.RegularExpressions;
using Batzill.Server.Core.Settings.Custom.Operations;
using System.Security;
using Batzill.Server.Core.Operations.CustomOperations.Authentication;

namespace Batzill.Server.Core.Operations
{
    public class AuthenticationOperation : Operation
    {
        private const string ParameterUserName = "username";
        private const string ParameterKey = "key";

        private const string CookieAccessToken = "AccessToken";

        private static AuthenticationManager AuthManager;
        private static bool HttpsOnly;

        public override string Name => "Authentication";

        public AuthenticationOperation() : base()
        {
        }

        public override void InitializeClass(OperationSettings settings)
        {
            if (!(settings is AuthenticationOperationSettings))
            {
                throw new ArgumentException($"Type '{settings.GetType()}' is invalid for this operation.");
            }

            AuthenticationOperationSettings internalSettings = settings as AuthenticationOperationSettings;

            /* initialize the AuthenticationManager */
            AuthenticationOperation.AuthManager = new AuthenticationManager(internalSettings.SessionDuration);
            
            foreach(Credentials creds in internalSettings.Credentials)
            {
                AuthenticationOperation.AuthManager.AddUser(creds);
            }

            /* store remaining properties */
            AuthenticationOperation.HttpsOnly = internalSettings.HttpsOnly;
        }

        public override void Execute(HttpContext context)
        {
            context.Response.SetDefaultValues();

            // Check for https

            this.logger?.Log(EventType.OperationInformation, $"Request protocol is '{context.Request.HttpMethod}'. Settings enfore https: '{AuthenticationOperation.HttpsOnly}'.");
            if (AuthenticationOperation.HttpsOnly && !context.Request.IsSecureConnection)
            {
                context.Response.StatusCode = 403;
                context.Response.WriteContent("Authentication is only enabled for https.");

                return;
            }

            // Create response content

            this.logger?.Log(EventType.OperationInformation, "Parsing Authentication Details.");

            var parameters = System.Web.HttpUtility.ParseQueryString(System.Web.HttpUtility.UrlDecode(context.Request.Url.Query));

            if(string.IsNullOrEmpty(parameters[AuthenticationOperation.ParameterUserName]) || string.IsNullOrEmpty(parameters[AuthenticationOperation.ParameterKey]))
            {
                context.Response.WriteContent("Please provide 'username' and 'key'.");
                return;
            }

            this.logger?.Log(EventType.OperationInformation, "Verifying credentials.");

            string accessToken = null;
            DateTime expirationDate = new DateTime();
            try
            {
                (accessToken, expirationDate) = AuthenticationOperation.AuthManager.AuthenticateUser(parameters[AuthenticationOperation.ParameterUserName], parameters[AuthenticationOperation.ParameterKey]);
            }
            catch(UnauthorizedAccessException ex)
            {
                context.Response.StatusCode = 403;
                return;
            }

            this.logger?.Log(EventType.OperationInformation, "Authenticaion was successful, returning access token.");

            context.Response.Cookies.Add(new System.Net.Cookie(AuthenticationOperation.CookieAccessToken, accessToken)
            {
                Expires = expirationDate
            });

            context.Response.WriteContent($"Authentication was successful. AccessToken: '{accessToken}', ValidUntil: '{expirationDate}'.");

            return;
        }

        public override bool Match(HttpContext context)
        {
            return Regex.IsMatch(context.Request.Url.AbsolutePath, @"^/auth", RegexOptions.IgnoreCase);
        }

        public static bool HandleAuthentication(HttpContext context)
        {
            // Check for https

            if (AuthenticationOperation.HttpsOnly && !context.Request.IsSecureConnection)
            {
                context.Response.StatusCode = 403;
                context.Response.WriteContent("Authentication is only enabled for https.");

                return false;
            }

            // First check in cookies

            string accessToken = context.Request.Cookies[AuthenticationOperation.CookieAccessToken]?.Value;

            // Check in headers

            if (string.IsNullOrEmpty(accessToken))
            {
                accessToken = context.Request.Headers[AuthenticationOperation.CookieAccessToken];
            }

            // check in query
            if (string.IsNullOrEmpty(accessToken))
            {
                var parameters = System.Web.HttpUtility.ParseQueryString(System.Web.HttpUtility.UrlDecode(context.Request.Url.Query));
                accessToken = parameters[AuthenticationOperation.CookieAccessToken];
            }
            
            if (!AuthenticationOperation.AuthManager.IsValidAccessToken(accessToken))
            {
                context.Response.StatusCode = 403;
                context.Response.WriteContent("Please authenticate at '/auth?username={USERNAME}&key={KEY}'.");

                return false;
            }

            return true;
        }
    }
}
