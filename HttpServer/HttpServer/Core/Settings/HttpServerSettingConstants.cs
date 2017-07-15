using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer.Core.Settings
{
    public static class HttpServerSettingConstants
    {
        public const char ListValueSplitter = ',';

        public const string MonitorSettingsFileChange = "MonitorSettingsFileChange";

        public const string Protocol = "Protocol";
        public const string Binding = "Bindings";
        public const string Port = "Port";

        public const string IdleTimeout = "IdleTimeout";
        public const string ConnectionLimit = "ConnectionLimit";
    }
}
