namespace Batzill.Server.Core.Logging
{
    public class AsyncLogWriter : ILogWriter
    {
        private ILogWriter logWriter;

        public AsyncLogWriter(ILogWriter logWriter)
        {
            this.logWriter = logWriter;
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
