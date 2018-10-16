using System;

namespace Batzill.Server.Core.Logging
{
    public class ConsoleLogWriter : ILogWriter
    {
        public const string Name = "Console";

        public void WriteLog(Log log)
        {
            Console.WriteLine(
                "[{0} | {1}] {2} {3}", 
                log.Timestamp, 
                log.EventType, 
                log.ExtendedData.Length > 1 ? $"('{string.Join("', '", log.ExtendedData, 0, log.ExtendedData.Length - 1)}')" : "",
                log.ExtendedData[log.ExtendedData.Length - 1]);
        }
    }
}
