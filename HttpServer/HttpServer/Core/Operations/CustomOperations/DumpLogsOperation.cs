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
    public class DumpLogsOperation : Operation
    {

        public override int Priority
        {
            get
            {
                return 1;
            }
        }

        public override string Name
        {
            get
            {
                return "DumpLogs";
            }
        }

        public DumpLogsOperation() : base()
        {
        }

        public override void Execute(HttpContext context)
        {
            context.Response.SetDefaultValues();

            this.logger.Log(EventType.OperationInformation, "Start dumping log file");
            
            string file = Path.Combine(settings.Get(HttpServerSettingNames.LogFolder), settings.Get(HttpServerSettingNames.LogFileName));
            using (SystemFileReader reader = new SystemFileReader(file))
            {
                context.Response.WriteContent(reader.ReadEntireFile());
            }
            
            this.logger.Log(EventType.OperationInformation, "Finished dimping logs, dumped {0:0.###}KB", (context.Response.ContentLength / ((double)1024)));

            return;
        }

        public override bool Match(HttpContext context)
        {
            return Regex.IsMatch(context.Request.RawUrl, "^/logs/?$", RegexOptions.IgnoreCase);
        }
    }
}
