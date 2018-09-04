using Batzill.Server.Core.Operations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Batzill.Server.Core.Settings.Custom.Operations
{
    public class ClientLoginOperationSettings : OperationSettings
    {
        public List<ClientLoginOperation.Client> Clients
        {
            get; set;
        }

        public override void Validate()
        {
            if (this.AuthenticationRequired)
            {
                throw new ArgumentException($"'{nameof(this.AuthenticationRequired)}' is not allowed for '{this.Name}'.");
            }

            if (this.Clients != null)
            {
                this.Clients.ForEach((c) => c.Validate());
            }
        }
    }
}
