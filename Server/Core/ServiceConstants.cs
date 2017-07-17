using System.Reflection;
using System.Runtime.InteropServices;

namespace Batzill.Server.Core
{
    public static class ServiceConstants
    {
        public static string ApplicationId = ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), true)[0]).Value;

        public const char ListValueSplitter = ',';

        public const int MinPort = 1024;
        public const int MaxPort = 65535;
        public const int DefaultPort = 80;
    }
}
