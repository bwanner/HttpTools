using System;
using System.IO;
using Batzill.Server.Core.IO;
using Batzill.Server.Core.Settings.Custom.Operations;

namespace Batzill.Server.Core.Logging
{
    public class OperationLogWriter : ILogWriter
    {
        public const string Name = "Operation";
        public const string OperationCollectionFolder = "All";

        private IFileWriter fileWriter;
        private string folder;

        public OperationLogWriter(IFileWriter fileWriter, OperationLogWriterSettings settings)
        {
            this.fileWriter = fileWriter;

            this.ApplySettings(settings);
        }
        
        public void ApplySettings(OperationLogWriterSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException($"'{nameof(settings)}' can't be null.");
            }

            if (!Directory.Exists(settings.LogFolder))
            {
                Directory.CreateDirectory(settings.LogFolder);
            }

            lock (this.fileWriter)
            {
                this.folder = settings.LogFolder;
            }
        }

        public void WriteLog(Log log)
        {
            if (log is OperationLog)
            {
                OperationLog fl = log as OperationLog;

                lock (this.fileWriter)
                {
                    string logEntry = string.Format("{0}, {1}, {2}", fl.Timestamp, fl.EventType, fl.Message);

                    // write logentry into /[OperationName]/[Guid]
                    string LogFolder1 = Path.Combine(folder, fl.OperationName);
                    string logFile1 = Path.Combine(LogFolder1, fl.OperationId + ".log");

                    if (!Directory.Exists(LogFolder1))
                    {
                        Directory.CreateDirectory(LogFolder1);
                    }

                    using (this.fileWriter.Open(logFile1))
                    {
                        this.fileWriter.WriteLine(logEntry);
                    }

                    // write logentry into /All/[Guid]
                    string LogFolder2 = Path.Combine(folder, OperationLogWriter.OperationCollectionFolder);
                    string logFile2 = Path.Combine(LogFolder2, fl.OperationId + ".log");

                    if (!Directory.Exists(LogFolder2))
                    {
                        Directory.CreateDirectory(LogFolder2);
                    }

                    using (this.fileWriter.Open(logFile2))
                    {
                        this.fileWriter.WriteLine(logEntry);
                    }
                }
            }
        }
    }
}
