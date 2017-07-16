using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Batzill.Server.Core.Logging
{
    public class FrontendOperationLog : Log
    {
        public string OperationId => base.ExtendedData[0];
        public string OperationName => base.ExtendedData[1];
        public string Message => base.ExtendedData[2];

        public FrontendOperationLog(EventType eventType, string operationId, string operationName, string message) : base(eventType, operationId, operationName, message)
        {
        }
    }
}
