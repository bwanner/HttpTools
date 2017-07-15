using System;
using System.Text;

namespace HttpServer.Core.Logging
{
    public class ConsoleLogWriter : ILogWriter
    {
        public void WriteLog(LogEntry log)
        {
            StringBuilder output = new StringBuilder();

            output.AppendFormat("[{0} | {1}] {2}", log.Timestamp, log.Type, log.Message);

            foreach (string data in log.ExtendedData)
            {
                output.Append(", ");
                output.Append(data);
            }

            Console.WriteLine(output.ToString());
        }
    }
}
