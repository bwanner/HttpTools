using Batzill.Server.Core.IO;
using Batzill.Server.Core.Logging;
using Batzill.Server.Core.Settings.Custom.Operations;

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
namespace Batzill.Server.Core.Settings
{
    public class HttpServerSettingsProvider : IDisposable
    {
        // regex expressions
        public const string RegexSettingsEntry = @"^([a-zA-Z0-9\-\.]+)(\+?) (.+)$";
        
        public HttpServerSettings Settings { get; private set; }

        private Logger logger;
        private IFileReader fileReader;

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

                        this.Settings = settings;
                    }
                }
                catch (Exception ex)
                {
                    this.logger?.Log(EventType.SettingsParsingError, "Unable to load settings from file '{0}': {1}", this.settingsFile, ex.ToString());
                    throw new Exception("Unable to load settings file.");
                }
            }
        }
    }
}
