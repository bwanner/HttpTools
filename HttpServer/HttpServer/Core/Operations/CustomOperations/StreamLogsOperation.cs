using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Batzill.Server.Core.IO;
using Batzill.Server.Core.Logging;
using Batzill.Server.Core.ObjectModel;
using Batzill.Server.Core.Settings;

namespace Batzill.Server.Core.Operations
{
    public class StreamLogsOperation : Operation
    {

        public override int Priority
        {
            get
            {
                return 3;
            }
        }

        public override string Name
        {
            get
            {
                return "StreamLogs";
            }
        }

        public StreamLogsOperation() : base()
        {
        }

        public override void Execute(HttpContext context)
        {
            int idleTimeout = 120000;

            context.Response.SetDefaultValues();
            context.Response.SendChuncked = true;
            context.SyncResponse();

            this.logger.Log(EventType.OperationInformation, "Start streaming log files");
            
            string file = Path.Combine(settings.Get(HttpServerSettingNames.LogFolder), settings.Get(HttpServerSettingNames.LogFileName));
            using (SystemFileReader reader = new SystemFileReader(file))
            {
                foreach (string line in reader.StreamLineByLine(true, idleTimeout, 500))
                {
                    context.Response.WriteContent(string.Format("{0}\r\n", line));
                    context.FlushResponse();
                }
            }
            
            this.logger.Log(EventType.OperationInformation, "No file changes for {0}ms, stopping the log stream.", idleTimeout);

            return;
        }

        public override bool Match(HttpContext context)
        {
            return Regex.IsMatch(context.Request.RawUrl, "^/stream/logs/?$", RegexOptions.IgnoreCase);
        }
    }
}
