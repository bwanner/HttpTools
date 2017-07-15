using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer.Core.Logging
{
    public class AsyncLogWriter : ILogWriter
    {
        private ILogWriter logWriter;

        public AsyncLogWriter(ILogWriter logWriter)
        {
            this.logWriter = logWriter;
        }

        public void WriteLog(LogEntry log)
        {
            lock (this.logWriter)
            {
                this.logWriter.WriteLog(log);
            }
        }
    }
}
