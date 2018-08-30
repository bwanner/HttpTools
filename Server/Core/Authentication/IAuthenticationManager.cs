﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Batzill.Server.Core.Authentication
{
    public interface IAuthenticationManager
    {
        bool HttpsOnly { get; }

        void AddUser(string userId);

        (string, DateTime) GetAccessToken(string userId, bool tryRefreshExistingSession = false);

        bool IsValidAccessToken(string accessToken);

        string GetUserId(string accessToken);
    }
}
