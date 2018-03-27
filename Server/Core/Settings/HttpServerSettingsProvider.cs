using Batzill.Server.Core.IO;
using Batzill.Server.Core.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;

namespace Batzill.Server.Core.Settings
{
    public class HttpServerSettingsProvider : IDisposable
    {
        // regex expressions
        public const string RegexSettingsEntry = @"^([a-zA-Z0-9\-\.]+)(\+?) (.+)$";

        public event EventHandler<HttpServerSettings> SettingsChanged;

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
        private FileSystemWatcher fileMonitor;
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
            lock (this)
            {
                try
                {
                    this.logger.Log(EventType.SystemSettings, "Load Settings from settings file.");

                    using (this.fileReader.Open(this.settingsFile))
                    {
                        int lineNumber = 1;
                        bool successful = true;
                        HttpServerSettings settings = new HttpServerSettings();

                        foreach (string line in this.fileReader.ReadLineByLine())
                        {
                            if(string.IsNullOrEmpty(line) || line.StartsWith("#"))
                            {
                                continue;
                            }

                            Match match = Regex.Match(line.Trim(), HttpServerSettingsProvider.RegexSettingsEntry, RegexOptions.IgnoreCase);

                            if (match.Success)
                            {
                                string name = match.Groups[1].Value;
                                bool append = !string.IsNullOrEmpty(match.Groups[2].Value);
                                string value = match.Groups[3].Value;

                                this.logger.Log(EventType.SystemSettings, "Found Setting '{0}', Value: '{1}' ({2})", name, value, append ? "append" : "set");

                                settings.Set(name, value, append);
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
                catch (Exception ex)
                {
                    this.logger.Log(EventType.SystemError, "Unable to load settings from file '{0}': {1}", this.settingsFile, ex.ToString());
                    throw new Exception("Unable to load settings file.");
                }
            }
        }

        public void ApplyNewSettings(HttpServerSettings settings)
        {
            lock (this)
            {
                this.logger.Log(EventType.SystemSettings, "Apply new settings.");

                this.settings = settings;

                this.monitorSettingsFile = string.Equals("true", this.settings.Get(HttpServerSettingNames.MonitorSettingsFileChange), StringComparison.InvariantCultureIgnoreCase);
                if (this.monitorSettingsFile)
                {
                    this.fileMonitor = new FileSystemWatcher(Path.GetDirectoryName(this.settingsFile));
                    this.fileMonitor.Filter = Path.GetFileName(this.settingsFile);
                    this.fileMonitor.NotifyFilter = NotifyFilters.LastWrite;
                    this.fileMonitor.Changed += this.FileMonitor_Changed;
                    this.fileMonitor.EnableRaisingEvents = true;
                }
                else if (fileMonitor != null)
                {
                    this.fileMonitor.EnableRaisingEvents = false;
                    fileMonitor.Dispose();
                }

                if (this.SettingsChanged != null)
                {
                    this.SettingsChanged.Invoke(this, this.settings);
                }

                this.logger.Log(EventType.SystemSettings, "Applied new settings.");
            }
        }

        private void FileMonitor_Changed(object sender, FileSystemEventArgs e)
        {
            this.logger.Log(EventType.SystemSettings, "Setting file changes detected, reloading file!");
            lock (this)
            {
                this.fileMonitor.EnableRaisingEvents = false;

                this.LoadSettings();

                Thread.Sleep(300);

                this.fileMonitor.EnableRaisingEvents = true;
            }
        }
    }
}
