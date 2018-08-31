using Batzill.Server.Core.Settings;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Batzill.Server.Core.Authentication
{
    public class AuthenticationManager : IAuthenticationManager
    {
        private ConcurrentBag<User> users;
        private ConcurrentDictionary<string, User> userIdMapping;
        private ConcurrentDictionary<string, User> accessTokenMapping;

        private int sessionValidityInMinutes = 60;
        private bool SessionRefreshAllowed;

        public AuthenticationManager(HttpServerSettings settings)
        {
            this.sessionValidityInMinutes = settings.Authentication.SessionDuration;
            this.SessionRefreshAllowed = settings.Authentication.SessionRefresh;

            this.users = new ConcurrentBag<User>();
            this.userIdMapping = new ConcurrentDictionary<string, User>(StringComparer.InvariantCultureIgnoreCase);
            this.accessTokenMapping = new ConcurrentDictionary<string, User>(StringComparer.InvariantCultureIgnoreCase);
        }

        public void AddUser(string userId)
        {
            if(this.userIdMapping.ContainsKey(userId))
            {
                throw new ArgumentException($"The user '{userId}' already exists!");
            }

            User user = new User()
            {
                Id = userId,
                Session = null
            };

            if(!this.userIdMapping.TryAdd(user.Id, user))
            {
                throw new Exception($"Adding user '{user.Id}' failed for unknown reasons.");
            }

            this.users.Add(user);
        }

        public (string, DateTime) GetAccessToken(string userId, bool tryRefreshExistingSession = false)
        {
            if(string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            User user = this.userIdMapping[userId];

            if (user.Session != null && user.Session.ValidUntil > DateTime.UtcNow)
            {
                if (tryRefreshExistingSession && this.SessionRefreshAllowed)
                {
                    user.Session.ValidUntil = DateTime.UtcNow.AddMinutes(this.sessionValidityInMinutes);
                }
            }
            else
            {
                user.Session = new Session()
                {
                    AccessToken = Guid.NewGuid().ToString(),
                    ValidUntil = DateTime.UtcNow.AddMinutes(this.sessionValidityInMinutes)
                };

                if (!this.accessTokenMapping.TryAdd(user.Session.AccessToken, user))
                {
                    user.Session = null;
                    throw new Exception($"Authentication for '{userId}' failed for unknown reasons.");
                }
            }

            // clean invalid accessTokenMapping entries here ...
            try
            {
                foreach (string accessToken in this.accessTokenMapping.Keys)
                {
                    if (this.accessTokenMapping[accessToken].Session == null
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

        public string GetUserId(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new ArgumentNullException(nameof(accessToken));
            }

            return this.accessTokenMapping[accessToken]?.Id;
        }

        public bool IsValidAccessToken(string accessToken)
        {
            return !string.IsNullOrEmpty(accessToken) 
                && this.accessTokenMapping.ContainsKey(accessToken)
                && this.accessTokenMapping[accessToken].Session != null
                && string.Equals(this.accessTokenMapping[accessToken].Session.AccessToken, accessToken, StringComparison.InvariantCultureIgnoreCase)
                && this.accessTokenMapping[accessToken].Session.ValidUntil > DateTime.UtcNow;
        }
    }
}
