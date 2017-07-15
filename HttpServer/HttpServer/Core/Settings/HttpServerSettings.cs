using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer.Core.Settings
{
    public class HttpServerSettings
    {
        private readonly Dictionary<string, string> settings;
        
        public string this[string name]
        {
            get
            {
                return this.Get(name);
            }
            set
            {
                this.Set(name, value);
            }
        }

        public HttpServerSettings()
        {
            this.settings = new Dictionary<string, string>();
        }

        public void Set(string name, string value)
        {
            name = this.PrepareName(name);

            if (!this.Contains(name))
            {
                this.settings.Add(name, value);
            }
            else
            {
                this.settings[name] = value;
            }
        }

        public string Get(string name)
        {
            name = this.PrepareName(name);

            if (!this.Contains(name))
            {
                return null;
            }

            return this.settings[name];
        }

        public bool Contains(string name)
        {
            name = this.PrepareName(name);

            return this.settings.ContainsKey(name);
        }

        public HttpServerSettings Clone()
        {
            HttpServerSettings result = new HttpServerSettings();
            foreach (KeyValuePair<string, string> entry in this.settings)
            {
                result.Set(entry.Key.ToString(), entry.Value.ToString());
            }

            return result;
        }

        private string PrepareName(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                return name.ToLowerInvariant();
            }

            return string.Empty;
        }
    }
}
