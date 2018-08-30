using Newtonsoft.Json;
using System;

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

        public virtual void Validate()
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                throw new NullReferenceException($"'{nameof(this.Name)}' can't be null or empty!");
            }
        }
    }
}
