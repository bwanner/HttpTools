using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Batzill.Server.Core.Settings.Custom.Operations
{
    public class AuthenticationRequiredOperationSettings : OperationSettings
    {
        private bool authenticationRequired = true;
        public bool AuthenticationRequired
        {
            get => this.authenticationRequired;
            set
            {
                this.authenticationRequired = value;
            }
        }
    }
}
