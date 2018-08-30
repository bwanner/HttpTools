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
            lock(this.logWriters)
            {
                this.logWriters.Add(logWriter);
            }
        }

        public void Remove(ILogWriter logWriter)
        {
            lock (this.logWriters)
            {
                this.logWriters.Remove(logWriter);
            }
        }

        public void WriteLog(Log log)
        {
            lock (this.logWriters)
            {
                foreach (ILogWriter logWriter in this.logWriters)
                {
                    logWriter.WriteLog(log);
                }
            }
        }
    }
}
