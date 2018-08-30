using System;
using Batzill.Server.Core.Logging;
using Batzill.Server.Core.ObjectModel;
using Batzill.Server.Core.Settings;
using System.Text.RegularExpressions;
using Batzill.Server.Core.Settings.Custom.Operations;
using Newtonsoft.Json;
using Batzill.Server.Core.Authentication;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace Batzill.Server.Core.Operations
{
    public class ClientLoginOperation : Operation
    {
        private static ConcurrentBag<string> WhiteList;
        private static bool HttpsOnly;

        public override string Name => "ClientLogin";

        public ClientLoginOperation(Logger logger = null) : base(logger)
        {
        }

        public override void InitializeClass(OperationSettings settings, IAuthenticationManager authManager)
        {
            if (!(settings is ClientLoginOperationSettings))
            {
                throw new ArgumentException($"Type '{settings.GetType()}' is invalid for this operation.");
            }

            ClientLoginOperationSettings internalSettings = (settings as ClientLoginOperationSettings);

            ClientLoginOperation.WhiteList = new ConcurrentBag<string>();
            foreach(string client in internalSettings.WhiteList)
            {
                this.logger?.Log(EventType.OperationClassInitalization, "Adding client '{0}' to white list.", client);

                ClientLoginOperation.WhiteList.Add(client);
                authManager.AddUser(ClientLoginOperation.GetUserId(client));
            }

            ClientLoginOperation.HttpsOnly = internalSettings.HttpsOnly;
            this.logger?.Log(EventType.OperationClassInitalization, "Use HttpsOnly for ClientLogin: '{0}'.", ClientLoginOperation.HttpsOnly);
        }

        protected override void ExecuteInternal(HttpContext context, IAuthenticationManager authManager)
        {
            context.Response.SetDefaultValues();

            // Check for https

            this.logger?.Log(EventType.OperationInformation, $"Request protocol is '{context.Request.HttpMethod}'. Settings enforce https: '{ClientLoginOperation.HttpsOnly}'.");
            if (ClientLoginOperation.HttpsOnly && !context.Request.IsSecureConnection)
            {
                this.logger?.Log(EventType.OperationError, "Authentication is only enabled for https while connection to client is http.");

                context.Response.StatusCode = 403;
                context.Response.WriteContent("Authentication is only enabled for https.");

                return;
            }

            // Create response content

            string client = context.Request.RemoteEndpoint.Address.ToString();

            this.logger?.Log(EventType.OperationInformation, "Check whitelist for clientIp '{0}'.", client);

            if(!ClientLoginOperation.WhiteList.Any((wlClient) => String.Equals(client, wlClient, StringComparison.InvariantCultureIgnoreCase)))
            {
                this.logger?.Log(EventType.OperationError, "Unknown client: '{0}'.", client);

                context.Response.StatusCode = 403;
                context.Response.WriteContent("Unknown client.");

                return;
            }

            this.logger?.Log(EventType.OperationInformation, "Found client '{0}' in whitelist.", client);

            string accessToken = null;
            DateTime expirationDate = new DateTime();
            try
            {
                (accessToken, expirationDate) = authManager.GetAccessToken(ClientLoginOperation.GetUserId(client), true);
            }
            catch(Exception ex)
            {
                this.logger?.Log(EventType.OperationError, "Exception occured while trying to get access token for client '{0}': '{1}'.", client, ex);

                context.Response.StatusCode = 403;
                context.Response.WriteContent("Unknown client.");

                return;
            }

            this.logger?.Log(EventType.OperationInformation, "Authentication was successful, returning access token.");

            context.Response.Cookies.Add(new System.Net.Cookie(AuthenticationRequiredOperation.AccessTokenName, accessToken)
            {
                Expires = expirationDate,
                Path = "/"
            });

            context.Response.WriteContent($"Authentication was successful. AccessToken: '{accessToken}', ValidUntil: '{expirationDate}'.");

            return;
        }

        private static string GetUserId(string client)
        {
            return Utils.CalculateMD5Hash(client.ToLowerInvariant());
        }

        public override bool Match(HttpContext context)
        {
            return Regex.IsMatch(context.Request.Url.AbsolutePath, @"^/auth/client$", RegexOptions.IgnoreCase);
        }
    }
}
