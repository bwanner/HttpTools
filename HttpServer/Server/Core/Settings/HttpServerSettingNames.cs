using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Batzill.Server.Core.Settings
{
    public static class HttpServerSettingNames
    {
        public const string MonitorSettingsFileChange = "MonitorSettingsFileChange";

        public const string Protocol = "Protocol";
        public const string Binding = "Bindings";
        public const string Port = "Port";

        public const string IdleTimeout = "IdleTimeout";
        public const string ConnectionLimit = "ConnectionLimit";

        public const string LogFolder = "LogFolder";
        public const string LogFileName = "LogFileName";
        public const string LogFolderOperations = "LogFolderOperations";
    }
}
