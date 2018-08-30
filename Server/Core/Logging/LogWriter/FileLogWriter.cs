using System;
using System.IO;
using Batzill.Server.Core.IO;
using Batzill.Server.Core.Settings;
using Batzill.Server.Core.Settings.Custom.Operations;

namespace Batzill.Server.Core.Logging
{
    public class FileLogWriter : ILogWriter
    {
        public const string Name = "File";

        private IFileWriter fileWriter;
        private string file;

        public FileLogWriter(IFileWriter fileWriter, FileLogWriterSettings settings)
        {
            this.fileWriter = fileWriter;

            this.ApplySettings(settings);
        }

        private void ApplySettings(FileLogWriterSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException($"'{nameof(settings)}' can't be null.");
            }

            if (!Directory.Exists(settings.LogFolder))
            {
                Directory.CreateDirectory(settings.LogFolder);
            }

            this.file = Path.Combine(settings.LogFolder, settings.FileName);
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
