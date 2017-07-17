using System;
using System.IO;

namespace Batzill.Server.Core.Settings
{
    public static class HttpServerSettingDefaults
    {
        public const string DefaultMonitorSettingsFileChange = "false";
        public const string DefaultEndpoint = "http:\\+:5555";

        public const string DefaultIdleTimeout = "60";
        public const string DefaultConnectionLimit = "20";

        public static string DefaultLogFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"logs\");
        public const string DefaultLogFileName = "logs.txt";
        public const string DefaultLogFolderOperations = "operations";
    }
}
