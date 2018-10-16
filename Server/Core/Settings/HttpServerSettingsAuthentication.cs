using System;
using System.Collections.Generic;
using System.Linq;
using Batzill.Server.Core.Authentication;

namespace Batzill.Server.Core.Settings
{
    public class HttpServerSettingsAuthentication
    {
        public int SessionDuration { get; set; } = 60;

        private bool sessionRefresh = false;
        public bool SessionRefresh
        {
            get => this.sessionRefresh;
            set
            {
                this.sessionRefresh = value;
            }
        }
        
        public List<User> Users
        {
            get; set;
        }

        public void Validate()
        {
            if (this.SessionDuration < 1)
            {
                throw new IndexOutOfRangeException($"'{nameof(this.SessionDuration)}' has to be at least 1.");
            }

            if(this.Users != null)
            {
                this.Users.ForEach(u => u.Validate());

                HashSet<string> userIds = new HashSet<string>(this.Users.Select(u => u.Id));

                if(userIds.Count != this.Users.Count)
                {
                    throw new ArgumentException($"'{nameof(this.Users)}' contains duplicates.");
                }
            }
        }
    }
}
