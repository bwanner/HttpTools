using System;
using Newtonsoft.Json;

namespace Batzill.Server.Core.Settings.Custom.Operations
{
    public class FileLogWriterSettings : LogWriterSettings
    {
        [JsonProperty(Required = Required.Always)]
        public string LogFolder
        {
            get; set;
        }

        private string fileName = "httpServer.log";
        public string FileName
        {
            get => this.fileName;
            set
            {
                this.fileName = value;
            }
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
