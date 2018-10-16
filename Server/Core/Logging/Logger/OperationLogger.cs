namespace Batzill.Server.Core.Logging
{
    public class OperationLogger : Logger
    {
        public string ClientIp
        {
            get; private set;
        }

        public string LocalPort
        {
            get; private set;
        }

        public string OperationId
        {
            get; private set;
        }

        public string OperationName
        {
            get; private set;
        }

        public string Url
        {
            get; private set;
        }

        public OperationLogger(ILogWriter logWriter, string clientIp, string localPort, string operationId, string operationName, string url) : base(logWriter)
        {
            this.ClientIp = clientIp;
            this.LocalPort = localPort;
            this.OperationId = operationId;
            this.OperationName = operationName;
            this.Url = url;
        }

        public override void Log(EventType type, string message = "")
        {
            this.LogWriter.WriteLog(new OperationLog(type, this.ClientIp, this.LocalPort, this.OperationId, this.OperationName, this.Url, message));
        }
    }
}
