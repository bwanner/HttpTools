using System;
using System.Text;

namespace Batzill.Server.Core.Logging
{
    public class ConsoleLogWriter : ILogWriter
    {
        public void WriteLog(Log log)
        {
            StringBuilder output = new StringBuilder();

            output.AppendFormat("[{0} | {1}] {2}", log.Timestamp, log.EventType, string.Join(", ", log.ExtendedData));
            Console.WriteLine(output.ToString());
        }
    }
}
