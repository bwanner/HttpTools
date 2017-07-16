using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Batzill.Server.Core.Logging
{
    public class BasicLogger : Logger
    {
        public BasicLogger(ILogWriter logWriter) : base(logWriter) { }

        public override void Log(EventType type, string message = "")
        {
            this.logWriter.WriteLog(new Log(type, message));
        }
    }
}
