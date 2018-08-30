using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Batzill.Server.Core.Settings
{
    public class HttpServerSettingsCore
    {
        [JsonProperty(Required = Required.Always)]
        public List<EndPoint> EndPoints
        {
            get; set;
        }

        private int idleTimeout = 30000;
        public int IdleTimeout
        {
            get => this.idleTimeout;
            set
            {
                this.idleTimeout = value;
            }
        }

        private int connectionLimit = 1000;
        public int ConnectionLimit
        {
            get => this.connectionLimit;
            set
            {
                this.connectionLimit = value;
            }
        }

        private bool httpKeepAlive = true;
        public bool HttpKeepAlive
        {
            get => this.httpKeepAlive;
            set
            {
                this.httpKeepAlive = value;
            }
        }

        private bool monitorSettingsFile = false;
        public bool MonitorSettingsFile
        {
            get => this.monitorSettingsFile;
            set
            {
                this.monitorSettingsFile = value;
            }
        }

        public void Validate()
        {
            if (this.EndPoints == null)
            {
                throw new NullReferenceException($"'{nameof(this.EndPoints)}' has to be specified.");
            }

            this.EndPoints.ForEach((ep) => ep.Validate());

            if(this.ConnectionLimit < 1)
            {
                throw new IndexOutOfRangeException($"'{nameof(this.ConnectionLimit)}' has to be at least 1.");
            }

            if (this.IdleTimeout < 1)
            {
                throw new IndexOutOfRangeException($"'{nameof(this.IdleTimeout)}' has to be at least 1.");
            }
        }
    }
}
