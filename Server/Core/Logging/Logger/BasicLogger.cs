namespace Batzill.Server.Core.Logging
{
    public class BasicLogger : Logger
    {
        public BasicLogger(ILogWriter logWriter) : base(logWriter) { }

        public override void Log(EventType type, string message = "")
        {
            this.LogWriter.WriteLog(new Log(type, message));
        }
    }
}
