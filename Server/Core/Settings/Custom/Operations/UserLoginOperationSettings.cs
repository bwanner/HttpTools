using Batzill.Server.Core.Operations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Batzill.Server.Core.Settings.Custom.Operations
{
    public class UserLoginOperationSettings : OperationSettings
    {
        [JsonProperty(Required = Required.Always)]
        public List<UserLoginOperation.Credentials> Credentials;

        private bool httpsOnly = true;
        public bool HttpsOnly
        {
            get => this.httpsOnly;
            set
            {
                this.httpsOnly = value;
            }
        }

        public override void Validate()
        {
            base.Validate();

            if(this.Credentials != null)
            {
                this.Credentials.ForEach((cred) => cred.Validate());
            }
        }
    }
}
