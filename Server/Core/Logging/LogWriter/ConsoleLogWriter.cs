using System;
using System.Text;
using Batzill.Server.Core.Settings;

namespace Batzill.Server.Core.Logging
{
    public class ConsoleLogWriter : ILogWriter
    {
        public bool ApplySettings(HttpServerSettings settings)
        {
            return true;
        }

        public void WriteLog(Log log)
        {
            StringBuilder output = new StringBuilder();

            output.AppendFormat("[{0} | {1}] {2}", log.Timestamp, log.EventType, string.Join(", ", log.ExtendedData));
            Console.WriteLine(output.ToString());
        }
    }
}
