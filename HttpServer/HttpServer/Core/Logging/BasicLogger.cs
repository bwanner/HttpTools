using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer.Core.Logging
{
    public class BasicLogger : Logger
    {
        private ILogWriter logWriter;

        public BasicLogger(ILogWriter logWriter)
        {
            this.logWriter = logWriter;
        }

        public override void Log(EventType type, string message)
        {
            this.logWriter.WriteLog(new LogEntry(type, message));
        }
    }
}
