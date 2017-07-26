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
        public const string InputRegex = @"^\/logs((\/?)|(\/operation\/([a-fA-F0-9\-]+)))$";

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

            Match match = Regex.Match(context.Request.RawUrl, DumpLogsOperation.InputRegex);
            if(match.Success)
            {
                string file = "";
                if(string.IsNullOrEmpty(match.Groups[4].Value))
                {
                    file = Path.Combine(settings.Get(HttpServerSettingNames.LogFolder), settings.Get(HttpServerSettingNames.LogFileName));
                }
                else
                {
                    file = Path.Combine(settings.Get(HttpServerSettingNames.LogFolder), settings.Get(HttpServerSettingNames.LogFolderOperations), OperationLogWriter.OperationCollectionFolder, string.Format("{0}.log", match.Groups[4].Value));
                }

                if(!File.Exists(file))
                {
                    this.logger.Log(EventType.OperationError, "Invalid request: '{0}'.", context.Request.RawUrl);
                    context.Response.WriteContent(string.Format("Invalid request: '{0}'. Check settings!", context.Request.RawUrl));
                    return;
                }

                this.logger.Log(EventType.OperationInformation, "Start dumping log file");

                using (SystemFileReader reader = new SystemFileReader(file))
                {
                    context.Response.WriteContent(reader.ReadEntireFile());
                }

                this.logger.Log(EventType.OperationInformation, "Finished dumping logs, dumped {0:0.###}KB", (context.Response.ContentLength / ((double)1024)));
            }
            else
            {
                this.logger.Log(EventType.OperationError, "Unknown request: '{0}'.", context.Request.RawUrl);
                context.Response.WriteContent(string.Format("Unknown request: '{0}'.", context.Request.RawUrl));
            }

            return;
        }

        public override bool Match(HttpContext context)
        {
            return Regex.IsMatch(context.Request.RawUrl, DumpLogsOperation.InputRegex, RegexOptions.IgnoreCase);
        }
    }
}
