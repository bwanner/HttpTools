using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HttpServer.Core.Logging
{
    public abstract class Logger
    {
        public void Log(EventType type, string format, params object[] args)
        {
            this.Log(type, string.Format(format, args));
        }

        public abstract void Log(EventType type, string message);
    }
}
