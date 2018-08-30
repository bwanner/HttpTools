using Batzill.Server.Core.Operations;
using Batzill.Server.Core.Operations.CustomOperations.Authentication;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Batzill.Server.Core.Settings.Custom.Operations
{
    public class AuthenticationOperationSettings : OperationSettings
    {
        [JsonProperty(Required = Required.Always)]
        public List<Credentials> Credentials;

        private bool httpsOnly = true;
        public bool HttpsOnly
        {
            get => this.httpsOnly;
            set
            {
                this.httpsOnly = value;
            }
        }

        private int sessionDuration = 60;
        public int SessionDuration
        {
            get => this.sessionDuration;
            set
            {
                this.sessionDuration = value;
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

        public class DynamicResponseEntry
        {
            [JsonProperty(Required = Required.Always)]
            public string Id
            {
                get; set;
            }

            [JsonProperty(Required = Required.Always)]
            public DynamicOperation.DynamicResponse Response
            {
                get; set;
            }

            public void Validate()
            {
                if (string.IsNullOrEmpty(this.Id))
                {
                    throw new NullReferenceException($"'{nameof(this.Id)}' can't be null or empty!");
                }

                if (this.Response == null)
                {
                    throw new NullReferenceException($"'{nameof(this.Response)}' can't be null!");
                }

                if (this.Response.StatusCode < 100 || this.Response.StatusCode > 999)
                {
                    throw new IndexOutOfRangeException($"'{nameof(this.Response.StatusCode)}' must be within [100, 999]!");
                }
            }
        }
    }
}
