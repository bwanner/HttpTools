using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Batzill.Server.Core.Operations.CustomOperations.Authentication
{
    public class AuthenticationManager
    {
        private ConcurrentBag<User> users;
        private ConcurrentDictionary<string, User> userNameMapping;
        private ConcurrentDictionary<string, User> accessTokenMapping;

        private int sessionValidityInMinutes = 60;

        public AuthenticationManager(int sessionValidityInMinutes)
        {
            this.sessionValidityInMinutes = sessionValidityInMinutes;

            this.users = new ConcurrentBag<User>();
            this.userNameMapping = new ConcurrentDictionary<string, User>(StringComparer.InvariantCultureIgnoreCase);
            this.accessTokenMapping = new ConcurrentDictionary<string, User>(StringComparer.InvariantCultureIgnoreCase);
        }

        public void AddUser(Credentials creds)
        {
            if(this.userNameMapping.ContainsKey(creds.UserName))
            {
                throw new ArgumentException($"The user '{creds.UserName}' already exists!");
            }

            User user = new User()
            {
                Credentials = creds,
                Session = null
            };

            if(!this.userNameMapping.TryAdd(creds.UserName, user))
            {
                throw new Exception($"Adding user '{creds.UserName}' failed for unknown reasons.");
            }

            this.users.Add(user);
        }

        public (string, DateTime) AuthenticateUser(string userName, string key)
        {
            if (!this.userNameMapping.ContainsKey(userName))
            {
                throw new UnauthorizedAccessException("Username or password invalid.");
            }

            string keyHash = CalculateMD5Hash(key);
            User user = this.userNameMapping[userName];

            if(!string.Equals(user.Credentials.KeyHash, keyHash, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new UnauthorizedAccessException("Username or password invalid.");
            }

            if(user.Session != null && user.Session.ValidUntil > DateTime.UtcNow)
            {
                return (user.Session.AccessToken, user.Session.ValidUntil);
            }

            user.Session = new Session()
            {
                AccessToken = Guid.NewGuid().ToString(),
                ValidUntil = DateTime.UtcNow.AddMinutes(this.sessionValidityInMinutes)
            };
            
            if (!this.accessTokenMapping.TryAdd(user.Session.AccessToken, user))
            {
                user.Session = null;
                throw new Exception($"Authentication for '{user.Credentials.UserName}' failed for unknown reasons.");
            }

            // clean invalid accessTokenMapping entries here ...
            try
            {
                foreach(string accessToken in this.accessTokenMapping.Keys)
                {
                    if(this.accessTokenMapping[accessToken].Session == null
                        || !string.Equals(accessToken, this.accessTokenMapping[accessToken].Session.AccessToken, StringComparison.InvariantCultureIgnoreCase)
                        || this.accessTokenMapping[accessToken].Session.ValidUntil <= DateTime.UtcNow)
                    {
                        this.accessTokenMapping.TryRemove(accessToken, out _);
                    }
                }
            }
            catch (Exception) { }

            return (user.Session.AccessToken, user.Session.ValidUntil);
        }

        public bool IsValidAccessToken(string accessToken)
        {
            return !string.IsNullOrEmpty(accessToken) 
                && this.accessTokenMapping.ContainsKey(accessToken)
                && this.accessTokenMapping[accessToken].Session != null
                && string.Equals(this.accessTokenMapping[accessToken].Session.AccessToken, accessToken, StringComparison.InvariantCultureIgnoreCase)
                && this.accessTokenMapping[accessToken].Session.ValidUntil > DateTime.UtcNow;
        }

        private static string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);

            byte[] hash = md5.ComputeHash(inputBytes);
            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)

            {
                sb.Append(hash[i].ToString("X2"));
            }

            return sb.ToString();
        }
    }
}
