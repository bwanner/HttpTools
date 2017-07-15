using HttpServer.Core.IO;
using HttpServer.Core.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HttpServer.Core.Settings
{
    public class HttpServerSettingsProvider : IDisposable
    {
        // regex expressions
        public const string RegexSettingsEntry = @"^([a-zA-Z0-9\-\.]+) (.+)$";

        public EventHandler<HttpServerSettings> SettingsChanged;

        private HttpServerSettings settings;
        public HttpServerSettings Settings
        {
            get
            {
                return this.settings.Clone();
            }
        }

        private Logger logger;
        private IFileReader fileReader;
        private bool monitorSettingsFile;

        private readonly string settingsFile;

        public HttpServerSettingsProvider(Logger logger, IFileReader fileReader, string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("Please provide a valid settings file, can't be emtpy.", "filePath");
            }
            if (!File.Exists(filePath))
            {
                throw new ArgumentException("Please prive a valid settings file and make sure it exists!", "filePath");
            }

            this.logger = logger;
            this.fileReader = fileReader;
            this.settingsFile = filePath;
            this.monitorSettingsFile = false;

            this.LoadSettings();
        }

        public void Dispose()
        {
            fileReader.Dispose();
        }

        public void LoadSettings()
        {
            try
            {
                using (this.fileReader.Open(this.settingsFile))
                {
                    int lineNumber = 1;
                    bool successful = true;
                    HttpServerSettings settings = new HttpServerSettings();

                    foreach (string line in this.fileReader.ReadLineByLine())
                    {
                        Match match = Regex.Match(line.TrimEnd(), HttpServerSettingsProvider.RegexSettingsEntry, RegexOptions.IgnoreCase);

                        if (match.Success && match.Groups.Count == 3)
                        {
                            string name = match.Groups[1].Value;
                            string value = match.Groups[2].Value;

                            this.logger.Log(EventType.SystemSettings, "Found Setting '{0}'. Value: '{1}'", name, value);

                            settings.Set(name, value);
                        }
                        else
                        {
                            this.logger.Log(EventType.SettingsParsingError, "Unable to parse line {0} in settingsfile '{1}'.", lineNumber, this.settingsFile);
                            successful = false;
                            break;
                        }

                        lineNumber++;
                    }

                    if (successful)
                    {
                        this.ApplyNewSettings(settings);
                    }

                }
            }
            catch(Exception ex)
            {
                this.logger.Log(EventType.SystemError, "Unable to load settings from file '{0}': {1}", this.settingsFile, ex.ToString());
                throw new Exception("Unable to load settings file.");
            }
        }

        public void ApplyNewSettings(HttpServerSettings settings)
        {
            this.logger.Log(EventType.SystemSettings, "Applying new settings.");

            this.settings = settings;

            if (this.settings.Contains(HttpServerSettingConstants.MonitorSettingsFileChange))
            {
                this.monitorSettingsFile = string.Equals("true", this.settings.Get(HttpServerSettingConstants.MonitorSettingsFileChange), StringComparison.InvariantCultureIgnoreCase);
            }

            if (this.SettingsChanged != null)
            {
                this.SettingsChanged.Invoke(this, this.settings);
            }

            this.logger.Log(EventType.SystemSettings, "Applied new settings.");
        }
    }
}
