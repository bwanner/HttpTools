using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Batzill.Server.Core.Logging
{
    public class OperationLogger : Logger
    {
        public string ClientIp
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

        public OperationLogger(ILogWriter logWriter, string clientIp, string operationId, string operationName) : base(logWriter)
        {
            this.ClientIp = clientIp;
            this.OperationId = operationId;
            this.OperationName = operationName;
        }

        public override void Log(EventType type, string message = "")
        {
            this.logWriter.WriteLog(new OperationLog(type, this.ClientIp, this.OperationId, this.OperationName, message));
        }
    }
}
