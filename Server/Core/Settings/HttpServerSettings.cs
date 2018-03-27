using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Batzill.Server.Core.Settings
{
    public class HttpServerSettings
    {
        private static Dictionary<string, string> defaultValues;
        private static Dictionary<string, string> DefaultValues
        {
            get
            {
                if (defaultValues == null)
                {
                    HttpServerSettings.LoadDefaultValues();
                }

                return HttpServerSettings.defaultValues;
            }
            set
            {
                HttpServerSettings.defaultValues = value;
            }
        }
        private static void LoadDefaultValues()
        {
            HttpServerSettings.defaultValues = new Dictionary<string, string>();
            foreach (FieldInfo fInfo in typeof(HttpServerSettingValues).GetFields().Where(x => x.Name.StartsWith("Default")))
            {
                HttpServerSettings.defaultValues.Add(fInfo.Name.ToLowerInvariant(), (string)fInfo.GetValue(null));
            }
        }

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

        public void Set(string name, string value, bool append = false)
        {
            name = this.PrepareName(name);

            if (!this.Contains(name, false))
            {
                this.settings.Add(name, value);
            }
            else if (append)
            {
                this.settings[name] = string.Format("{0}{1}{2}", this.settings[name], Environment.NewLine, value);
            }
            else
            {
                this.settings[name] = value;
            }
        }

        public string Get(string name, bool checkDefault = true)
        {
            name = this.PrepareName(name);

            if (this.settings.ContainsKey(name))
            {
                return this.settings[name];
            }
            else if (checkDefault)
            {
                return this.Default(name);
            }

            return null;
        }

        public IEnumerable<string> GetAll(string name, bool checkDefault = true)
        {
            return this.Get(name, checkDefault)?.Split(
                new string[]
                {
                    Environment.NewLine
                },
                StringSplitOptions.RemoveEmptyEntries);
        }

        public string Default(string name)
        {
            name = this.PrepareDefaultName(name);

            if (HttpServerSettings.DefaultValues.ContainsKey(name))
            {
                return HttpServerSettings.DefaultValues[name];
            }

            return "";
        }

        public bool Contains(string name, bool checkDefault = true)
        {
            name = this.PrepareName(name);

            if (this.settings.ContainsKey(name))
            {
                return true;
            }
            else if (checkDefault)
            {
                return this.ContainsDefault(name);
            }

            return false;
        }

        private bool ContainsDefault(string name)
        {
            name = this.PrepareDefaultName(name);
            return HttpServerSettings.DefaultValues.ContainsKey(name);
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

        private string PrepareDefaultName(string name)
        {
            return string.Format("default{0}", this.PrepareName(name));
        }
    }
}
