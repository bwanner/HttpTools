using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Batzill.Server.Core.Operations.CustomOperations.Authentication
{
    public class Credentials
    {
        [JsonProperty(Required = Required.Always)]
        public string UserName
        {
            get; set;
        }

        [JsonProperty(Required = Required.Always)]
        public string KeyHash
        {
            get; set;
        }

        public string ClientIp
        {
            get; set;
        }

        public void Validate()
        {
            if (string.IsNullOrEmpty(this.UserName))
            {
                throw new NullReferenceException($"'{nameof(this.UserName)}' can't be null or empty!");
            }

            if (string.IsNullOrEmpty(this.KeyHash))
            {
                throw new NullReferenceException($"'{nameof(this.KeyHash)}' can't be null or empty!");
            }
        }
    }
}
