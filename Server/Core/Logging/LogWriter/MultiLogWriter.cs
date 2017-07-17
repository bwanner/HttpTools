using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Batzill.Server.Core.Settings;

namespace Batzill.Server.Core.Logging
{
    public class MultiLogWriter : ILogWriter
    {
        private List<ILogWriter> logWriters;

        public MultiLogWriter()
        {
            this.logWriters = new List<ILogWriter>();
        }

        public MultiLogWriter(List<ILogWriter> logWriters)
        {
            this.logWriters = logWriters;
        }

        public void Add(ILogWriter logWriter)
        {
            this.logWriters.Add(logWriter);
        }

        public bool ApplySettings(HttpServerSettings settings)
        {
            bool result = true;
            foreach (ILogWriter writer in this.logWriters)
            {
                result &= writer.ApplySettings(settings);
            }

            return result;
        }

        public void WriteLog(Log log)
        {
            foreach (ILogWriter logWriter in this.logWriters)
            {
                logWriter.WriteLog(log);
            }
        }
    }
}
