using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer.Core.Logging
{
    public class LogEntry
    {
        public string[] ExtendedData
        {
            get;
            private set;
        }

        public DateTime Timestamp
        {
            get;
            private set;
        }

        public EventType Type
        {
            get;
            private set;
        }

        public string Message
        {
            get;
            private set;
        }

        public LogEntry(EventType type, string message = "", params string[] extendedData)
        {
            this.Timestamp = DateTime.Now;
            this.Type = type;
            this.Message = message;
            this.ExtendedData = extendedData ?? new string[0];
        }
    }
}
