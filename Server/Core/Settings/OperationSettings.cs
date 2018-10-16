using System;
using Newtonsoft.Json;

namespace Batzill.Server.Core.Settings
{
    public abstract class OperationSettings
    {
        [JsonProperty(Required = Required.Always)]
        public string Name
        {
            get; private set;
        }

        [JsonProperty(Required = Required.Always)]
        public int Priority
        {
            get; private set;
        }

        private bool authenticationRequired = false;
        public bool AuthenticationRequired
        {
            get => this.authenticationRequired;
            set
            {
                this.authenticationRequired = value;
            }
        }

        private bool httpsOnly = false;
        public bool HttpsOnly
        {
            get => this.httpsOnly;
            set
            {
                this.httpsOnly = value;
            }
        }

        public virtual void Validate()
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                throw new NullReferenceException($"'{nameof(this.Name)}' can't be null or empty!");
            }
        }
    }
}
