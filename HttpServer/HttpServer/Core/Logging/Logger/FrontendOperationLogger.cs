using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Batzill.Server.Core.Logging
{
    public class FrontendOperationLogger : Logger
    {
        public string OperationId
        {
            get; private set;
        }

        public string OperationName
        {
            get; private set;
        }

        public FrontendOperationLogger(ILogWriter logWriter, string operationId, string operationName) : base(logWriter)
        {
            this.OperationId = operationId;
            this.OperationName = operationName;
        }

        public override void Log(EventType type, string message = "")
        {
            this.logWriter.WriteLog(new FrontendOperationLog(type, this.OperationId, this.OperationName, message));
        }
    }
}
