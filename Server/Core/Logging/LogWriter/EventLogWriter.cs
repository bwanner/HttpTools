using System;
using Batzill.Server.Core.Settings;

namespace Batzill.Server.Core.Logging
{
    public class EventLogWriter : ILogWriter
    {
        public event EventHandler<Log> LogWritten;

        public void WriteLog(Log log)
        {
            if(this.LogWritten != null)
            {
                this.LogWritten.Invoke(this, log);
            }
        }
    }
}
