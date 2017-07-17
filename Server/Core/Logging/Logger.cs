using System;
using Batzill.Server.Core.Settings;

namespace Batzill.Server.Core.Logging
{
    public abstract class Logger : ISettingsChangable
    {
        public ILogWriter logWriter
        {
            get;
            private set;
        }

        public Logger(ILogWriter logWriter)
        {
            this.logWriter = logWriter;
        }

        public void Log(EventType type, string format, params object[] args)
        {
            this.Log(type, string.Format(format, args));
        }

        public virtual bool ApplySettings(HttpServerSettings settings)
        {
            return this.logWriter.ApplySettings(settings);
        }

        public abstract void Log(EventType type, string message = "");
    }
}
