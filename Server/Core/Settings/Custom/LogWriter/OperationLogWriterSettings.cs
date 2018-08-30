using Newtonsoft.Json;
using System;

namespace Batzill.Server.Core.Settings.Custom.Operations
{
    public class OperationLogWriterSettings : LogWriterSettings
    {
        [JsonProperty(Required = Required.Always)]
        public string LogFolder
        {
            get; set;
        }

        public override void Validate()
        {
            base.Validate();

            if (string.IsNullOrEmpty(this.LogFolder))
            {
                throw new NullReferenceException($"'{nameof(this.LogFolder)}' can't be null or empty!");
            }
        }
    }
}
