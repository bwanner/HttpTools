using System;
using System.IO;
using System.Text;
using Batzill.Server.Core.IO;
using Batzill.Server.Core.Settings;

namespace Batzill.Server.Core.Logging
{
    public class FileLogWriter : ILogWriter
    {
        private IFileWriter fileWriter;
        private string file;

        public FileLogWriter(IFileWriter fileWriter, HttpServerSettings settings)
        {
            this.fileWriter = fileWriter;
            this.ApplySettings(null, settings);

            settings.Provider.SettingsChanged += this.ApplySettings;
        }

        public void ApplySettings(object sender, HttpServerSettings settings)
        {
            lock (this.fileWriter)
            {
                this.file = Path.Combine(settings.Get(HttpServerSettingNames.LogFolder), settings.Get(HttpServerSettingNames.LogFileName));
            }
        }

        public void WriteLog(Log log)
        {
            lock (this.fileWriter)
            {
                string output = string.Format("[{0} | {1}] {2}", log.Timestamp, log.EventType, string.Join(", ", log.ExtendedData));

                using (this.fileWriter.Open(file))
                {
                    this.fileWriter.WriteLine(output);
                }
            }
        }
    }
}
