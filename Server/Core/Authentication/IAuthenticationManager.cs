using System;

namespace Batzill.Server.Core.Authentication
{
    public interface IAuthenticationManager
    {
        void AddUser(string userId);

        bool UserExists(string userId); 

        (string, DateTime) GetAccessToken(string userId, bool tryRefreshExistingSession = false);

        bool IsValidAccessToken(string accessToken);

        string GetUserId(string accessToken);
    }
}
