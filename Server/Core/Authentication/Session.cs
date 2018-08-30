using System;

namespace Batzill.Server.Core.Authentication
{
    public class Session
    {
        public DateTime ValidUntil
        {
            get; set;
        }

        public string AccessToken
        {
            get; set;
        }
    }
}
