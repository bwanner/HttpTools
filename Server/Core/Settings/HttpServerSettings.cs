using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Batzill.Server.Core.Settings
{
    public class HttpServerSettings
    {
        [JsonProperty(Required = Required.Always)]
        public HttpServerSettingsCore Core
        {
            get; set;
        }
        
        public HttpServerSettingsAuthentication Authentication
        {
            get; set;
        }

        [JsonProperty(Required = Required.Always)]
        public List<LogWriterSettings> Loggers
        {
            get; set;
        }

        [JsonProperty(Required = Required.Always)]
        public List<OperationSettings> Operations
        {
            get; set;
        }

        public void Validate()
        {
            if(this.Core == null)
            {
                throw new NullReferenceException($"'{nameof(this.Core)}' has to be specified.");
            }

            this.Core.Validate();

            if (this.Loggers == null)
            {
                throw new NullReferenceException($"'{nameof(this.Loggers)}' has to be specified.");
            }

            this.Loggers.ForEach((lw) => lw.Validate());

            if (this.Operations == null)
            {
                throw new NullReferenceException($"'{nameof(this.Operations)}' has to be specified.");
            }

            this.Operations.ForEach((op) => op.Validate());
        }
    }
}
