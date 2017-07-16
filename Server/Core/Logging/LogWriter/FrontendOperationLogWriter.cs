using Batzill.Server.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Batzill.Server.Core.Settings;

namespace Batzill.Server.Core.Logging
{
    public class FrontendOperationLogWriter : ILogWriter
    {
        private IFileWriter fileWriter;
        private string folder;

        public FrontendOperationLogWriter(IFileWriter fileWriter, HttpServerSettings settings)
        {
            this.fileWriter = fileWriter;
            this.ApplySettings(null, settings);

            settings.Provider.SettingsChanged += this.ApplySettings;
        }

        public void ApplySettings(object sender, HttpServerSettings settings)
        {
            lock (this.fileWriter)
            {
                this.folder = Path.Combine(settings.Get(HttpServerSettingNames.LogFolder), settings.Get(HttpServerSettingNames.LogFolderOperations));
            }
        }

        public void WriteLog(Log log)
        {
            if (log is FrontendOperationLog)
            {
                FrontendOperationLog fl = log as FrontendOperationLog;

                lock (this.fileWriter)
                {
                    string logEntry = string.Format("{0}, {1}, {2}", fl.Timestamp, fl.EventType, fl.Message);

                    string LogFolder = Path.Combine(folder, fl.OperationName);
                    string logFile = Path.Combine(LogFolder, fl.OperationId + ".log");

                    if (!Directory.Exists(LogFolder))
                    {
                        Directory.CreateDirectory(LogFolder);
                    }

                    using (this.fileWriter.Open(logFile))
                    {
                        this.fileWriter.WriteLine(logEntry);
                    }
                }
            }
        }
    }
}
