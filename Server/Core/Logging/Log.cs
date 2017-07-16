using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Batzill.Server.Core.Logging
{
    public class Log
    {
        public string[] ExtendedData
        {
            get;
            protected set;
        }

        public DateTime Timestamp
        {
            get;
            private set;
        }

        public EventType EventType
        {
            get;
            private set;
        }

        protected Log()
        {
            this.Timestamp = DateTime.Now;
        }

        public Log(EventType eventType, params string[] extendedData)
        {
            this.EventType = eventType;
            this.ExtendedData = extendedData ?? new string[0];

            this.Timestamp = DateTime.Now;
        }
    }
}
