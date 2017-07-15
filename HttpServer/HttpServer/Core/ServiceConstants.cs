using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer.Core
{
    public static class ServiceConstants
    {
        // shared values
        public const int MinPort = 1024;
        public const int MaxPort = 65535;
        public const int DefaultPort = 5555;

        public const string DefaultProtocol = "http";

        public const int DefaultIdleTimeout = 60;
        public const int DefaultMaxConcurrentConnections = 20;
    }
}
