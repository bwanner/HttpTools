using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Batzill.Server.Core.Settings
{
    public class HttpServerSettingsAuthentication
    {

        private int sessionDuration = 60;
        public int SessionDuration
        {
            get => this.sessionDuration;
            set
            {
                this.sessionDuration = value;
            }
        }

        private bool sessionRefresh = false;
        public bool SessionRefresh
        {
            get => this.sessionRefresh;
            set
            {
                this.sessionRefresh = value;
            }
        }

        public void Validate()
        {
            if (this.SessionDuration < 1)
            {
                throw new IndexOutOfRangeException($"'{nameof(this.SessionDuration)}' has to be at least 1.");
            }
        }
    }
}
