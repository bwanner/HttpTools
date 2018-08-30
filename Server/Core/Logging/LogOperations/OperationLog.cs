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
        public string LocalPort => base.ExtendedData[1];
        public string OperationId => base.ExtendedData[2];
        public string OperationName => base.ExtendedData[3];
        public string Url => base.ExtendedData[4];
        public string Message => base.ExtendedData[5];

        public OperationLog(EventType eventType, string clientIp, string localPort, string operationId, string operationName, string url, string message) 
            : base(eventType, clientIp, localPort, operationId, operationName, url, message)
        {
        }
    }
}
