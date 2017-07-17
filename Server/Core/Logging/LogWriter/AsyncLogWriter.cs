using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Batzill.Server.Core.Settings;

namespace Batzill.Server.Core.Logging
{
    public class AsyncLogWriter : ILogWriter
    {
        private ILogWriter logWriter;

        public AsyncLogWriter(ILogWriter logWriter)
        {
            this.logWriter = logWriter;
        }

        public bool ApplySettings(HttpServerSettings settings)
        {
            return this.logWriter.ApplySettings(settings);
        }

        public void WriteLog(Log log)
        {
            lock (this.logWriter)
            {
                this.logWriter.WriteLog(log);
            }
        }
    }
}
