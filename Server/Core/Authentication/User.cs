using Newtonsoft.Json;
using System;

namespace Batzill.Server.Core.Authentication
{
    public class User
    {
        public string Id
        {
            get; set;
        }

        [JsonIgnore]
        public Session Session
        {
            get; set;
        }

        public void Validate()
        {
            if(string.IsNullOrEmpty(Id))
            {
                throw new NullReferenceException($"'{nameof(this.Id)}' can't be null or empty!");
            }
        }
    }
}
