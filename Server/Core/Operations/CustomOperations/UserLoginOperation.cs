using System;
using Batzill.Server.Core.Logging;
using Batzill.Server.Core.ObjectModel;
using Batzill.Server.Core.Settings;
using System.Text.RegularExpressions;
using Batzill.Server.Core.Settings.Custom.Operations;
using Newtonsoft.Json;
using Batzill.Server.Core.Authentication;
using Batzill.Server.Core.Exceptions;
using System.Collections.Concurrent;

namespace Batzill.Server.Core.Operations
{
    public class UserLoginOperation : Operation
    {
        private const string ParameterUserName = "username";
        private const string ParameterKey = "key";

        private static ConcurrentBag<Credentials> Creds;
        private static bool HttpsOnly;

        public override string Name => "UserLogin";

        public UserLoginOperation(Logger logger = null) : base(logger)
        {
        }

        protected override void InitializeClassInternal(OperationSettings settings, IAuthenticationManager authManager)
        {
            if (!(settings is UserLoginOperationSettings))
            {
                throw new ArgumentException($"Type '{settings.GetType()}' is invalid for this operation.");
            }

            UserLoginOperationSettings internalSettings = (settings as UserLoginOperationSettings);

            UserLoginOperation.Creds = new ConcurrentBag<Credentials>();
            foreach(Credentials creds in internalSettings.Credentials)
            {
                this.logger?.Log(EventType.OperationClassInitalization, "Adding user '{0}'.", creds.UserName);

                UserLoginOperation.Creds.Add(creds);
                authManager.AddUser(UserLoginOperation.GetUserId(creds));
            }

            UserLoginOperation.HttpsOnly = internalSettings.HttpsOnly;
            this.logger?.Log(EventType.OperationClassInitalization, "Use HttpsOnly for UserLogin: '{0}'.", UserLoginOperation.HttpsOnly);
        }

        protected override void ExecuteInternal(HttpContext context, IAuthenticationManager authManager)
        {
            context.Response.SetDefaultValues();

            // Create response content

            this.logger?.Log(EventType.OperationAuthentication, "Parsing Authentication Details.");

            var parameters = System.Web.HttpUtility.ParseQueryString(System.Web.HttpUtility.UrlDecode(context.Request.Url.Query));

            string userName = parameters[UserLoginOperation.ParameterUserName];
            string key = parameters[UserLoginOperation.ParameterKey];

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(key))
            {
                this.logger?.Log(EventType.OperationAuthenticationError, "Username or key is not provided.");

                throw new UnauthorizedException("Please provide 'username' and 'key'.");
            }

            this.logger?.Log(EventType.OperationAuthentication, "Verifying credentials.");

            string userId = null;
            string keyHash = Utils.GenerateKeyHash(key);

            foreach (Credentials creds in UserLoginOperation.Creds)
            {
                if(string.Equals(creds.UserName, userName, StringComparison.InvariantCultureIgnoreCase))
                {
                    if(!string.Equals(creds.KeyHash, keyHash, StringComparison.InvariantCultureIgnoreCase))
                    {
                        this.logger?.Log(EventType.OperationAuthenticationError, "Invalid password provided.");

                        throw new UnauthorizedException("Username or password invalid.");
                    }

                    userId = UserLoginOperation.GetUserId(creds);
                    break;
                }
            }

            if (string.IsNullOrEmpty(userId))
            {
                this.logger?.Log(EventType.OperationAuthenticationError, "User '{0}' wasn't found.", userName);

                throw new UnauthorizedException("Username or password invalid.");
            }

            this.logger?.Log(EventType.OperationAuthentication, "Get access token.");

            string accessToken = null;
            DateTime expirationDate = new DateTime();
            try
            {
                (accessToken, expirationDate) = authManager.GetAccessToken(userId, true);
            }
            catch(Exception ex)
            {
                this.logger?.Log(EventType.OperationAuthenticationError, "Exception occured while trying to get access token for user '{0}': '{1}'.", userName, ex);

                throw new UnauthorizedException("Username or password invalid.");
            }

            this.logger?.Log(EventType.OperationAuthentication, "Authentication was successful, returning access token.");

            context.Response.Cookies.Add(new System.Net.Cookie(Operation.AccessTokenName, accessToken)
            {
                Expires = expirationDate,
                Path = "/"
            });

            context.Response.WriteContent($"Authentication was successful. AccessToken: '{accessToken}', ValidUntil: '{expirationDate}'.");

            return;
        }

        private static string GetUserId(Credentials creds)
        {
            return Utils.CalculateMD5Hash(creds.UserName.ToLowerInvariant());
        }

        public override bool Match(HttpContext context)
        {
            return Regex.IsMatch(context.Request.Url.AbsolutePath, @"^/auth/user$", RegexOptions.IgnoreCase);
        }

        public class Credentials
        {
            [JsonProperty(Required = Required.Always)]
            public string UserName
            {
                get; set;
            }

            [JsonProperty(Required = Required.Always)]
            public string KeyHash
            {
                get; set;
            }

            public void Validate()
            {
                if (string.IsNullOrEmpty(this.UserName))
                {
                    throw new NullReferenceException($"'{nameof(this.UserName)}' can't be null or empty!");
                }

                if (string.IsNullOrEmpty(this.KeyHash))
                {
                    throw new NullReferenceException($"'{nameof(this.KeyHash)}' can't be null or empty!");
                }
            }
        }
    }
}
