using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Batzill.Server.Core.Logging
{
    public class OperationLog : Log
    {
        public string ClientIp => base.ExtendedData[0];
        public string OperationId => base.ExtendedData[1];
        public string OperationName => base.ExtendedData[2];
        public string Message => base.ExtendedData[3];

        public OperationLog(EventType eventType, string clientIp, string operationId, string operationName, string message) : base(eventType, clientIp, operationId, operationName, message)
        {
        }
    }
}
