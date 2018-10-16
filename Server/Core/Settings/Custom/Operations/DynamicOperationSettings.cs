using System;
using System.Collections.Generic;
using Batzill.Server.Core.Operations;
using Newtonsoft.Json;

namespace Batzill.Server.Core.Settings.Custom.Operations
{
    public class DynamicOperationSettings : OperationSettings
    {
        public List<DynamicResponseEntry> Responses;

        public override void Validate()
        {
            base.Validate();

            if(this.Responses != null)
            {
                this.Responses.ForEach((resp) => resp.Validate());
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

                this.Response.Validate();
            }
        }
    }
}
