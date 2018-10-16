using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Batzill.Server.Core.IO;
using Batzill.Server.Core.Logging;
using Batzill.Server.Core.Settings.Custom.Operations;
using Newtonsoft.Json;

namespace Batzill.Server.Core.Settings
{
    public class HttpServerSettingsProvider : IDisposable
    {
        // regex expressions
        public const string RegexSettingsEntry = @"^([a-zA-Z0-9\-\.]+)(\+?) (.+)$";

        // To-DO enable dynamic reloading of settings ...
        private event EventHandler<HttpServerSettings> SettingsChanged;
        public HttpServerSettings Settings { get; private set; }

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
                throw new ArgumentException("Please provide a valid settings file and make sure it exists!", "filePath");
            }

            this.logger = logger;
            this.fileReader = fileReader;
            this.settingsFile = filePath;
            this.monitorSettingsFile = false;
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
                    this.logger?.Log(EventType.SystemSettings, "Load Settings from settings file.");

                    using (this.fileReader.Open(this.settingsFile))
                    {
                        HttpServerSettings settings = JsonConvert.DeserializeObject<HttpServerSettings>(
                            fileReader.ReadEntireFile(),
                            new JsonSerializerSettings()
                            {
                                Converters = new List<JsonConverter>()
                                {
                                    new OperationSettingsConverter(),
                                    new LogWriterSettingsConverter()
                                }
                            });

                        settings.Validate();

                        this.ApplyNewSettings(settings);
                    }
                }
                catch (Exception ex)
                {
                    this.logger?.Log(EventType.SettingsParsingError, "Unable to load settings from file '{0}': {1}", this.settingsFile, ex.ToString());
                    throw new Exception("Unable to load settings file.");
                }
            }
        }

        private void ApplyNewSettings(HttpServerSettings settings)
        {
            lock (this)
            {
                this.logger?.Log(EventType.SystemSettings, "Apply new settings.");

                this.Settings = settings;
                
                if (this.Settings.Core.MonitorSettingsFile)
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

                this.logger?.Log(EventType.SystemSettings, "Invoke SettingsChanged event.");

                if (this.SettingsChanged != null)
                {
                    this.SettingsChanged.Invoke(this, this.Settings);
                }

                this.logger?.Log(EventType.SystemSettings, "Applied new settings.");
            }
        }

        private void FileMonitor_Changed(object sender, FileSystemEventArgs e)
        {
            this.logger?.Log(EventType.SystemSettings, "Setting file changes detected, reloading file!");
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
