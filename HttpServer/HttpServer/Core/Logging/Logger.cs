namespace Batzill.Server.Core.Logging
{
    public abstract class Logger
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

        public abstract void Log(EventType type, string message = "");
    }
}
