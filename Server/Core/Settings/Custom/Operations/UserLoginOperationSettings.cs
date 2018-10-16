﻿using System;
using System.Collections.Generic;
using Batzill.Server.Core.Operations;
using Newtonsoft.Json;

namespace Batzill.Server.Core.Settings.Custom.Operations
{
    public class UserLoginOperationSettings : OperationSettings
    {
        [JsonProperty(Required = Required.Always)]
        public List<UserLoginOperation.Credentials> Credentials
        {
            get; set;
        }

        public override void Validate()
        {
            base.Validate();

            if(this.AuthenticationRequired)
            {
                throw new ArgumentException($"'{nameof(this.AuthenticationRequired)}' is not allowed for '{this.Name}'.");
            }

            if(this.Credentials != null)
            {
                this.Credentials.ForEach((cred) => cred.Validate());
            }
        }
    }
}
