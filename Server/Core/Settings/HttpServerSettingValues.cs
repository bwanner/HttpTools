using System;
using System.IO;

namespace Batzill.Server.Core.Settings
{
    public static class HttpServerSettingValues
    {
        public const string DefaultMonitorSettingsFileChange = "false";
        public const string DefaultEndpoint = "http:\\+:5555";

        public const string DefaultIdleTimeout = "60";
        public const string DefaultConnectionLimit = "20";
        public const string DefaultHttpKeepAlive = "true";

        public const string DefaultLogWriter = HttpServerSettingValues.LogWriterConsole;
        public const string LogWriterConsole = "Console";
        public const string LogWriterFile = "File";
        public const string LogWriterOperation = "Operation";

        public static string DefaultLogFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"logs\");
        public const string DefaultLogFileName = "logs.txt";
        public const string DefaultLogFolderOperations = "operations";
    }
}
