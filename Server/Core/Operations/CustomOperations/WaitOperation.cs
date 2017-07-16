using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Batzill.Server.Core.Logging;
using Batzill.Server.Core.ObjectModel;
using Batzill.Server.Core.Settings;

namespace Batzill.Server.Core.Operations
{
    public class WaitOperation : Operation
    {
        private const string InputRegex = "^/wait/?([0-9]*)$";

        public override int Priority
        {
            get
            {
                return 2;
            }
        }

        public override string Name
        {
            get
            {
                return "Wait";
            }
        }

        public WaitOperation() : base()
        {
        }

        public override void Execute(HttpContext context)
        {
            context.Response.SetDefaultValues();

            this.logger.Log(EventType.OperationInformation, "Got request to wait, try parsing seconds passed by client.");

            Match result = System.Text.RegularExpressions.Regex.Match(context.Request.RawUrl, WaitOperation.InputRegex, RegexOptions.IgnoreCase);
            if (result.Success && result.Groups.Count == 2 && !string.IsNullOrEmpty(result.Groups[1].Value))
            {
                if (Int32.TryParse(result.Groups[1].Value, out int waitInSec))
                {
                    this.logger.Log(EventType.OperationInformation, "Will sleep for {0} seconds before replying.", waitInSec);

                    Thread.Sleep(waitInSec * 1000);

                    this.logger.Log(EventType.OperationInformation, "Slept for {0} seconds, reply to client.", waitInSec);

                    context.Response.WriteContent(string.Format("Slept for {0} seconds.", waitInSec));
                }
                else
                {
                    this.logger.Log(EventType.OperationInformation, "Unable to parse '{0}' to an integer.", result.Groups[1].Value);

                    context.Response.WriteContent(string.Format("Unable to parse '{0}' to an integer.", result.Groups[1].Value));
                }
            }
            else
            {
                this.logger.Log(EventType.OperationInformation, "No number passed, return info page.");

                context.Response.WriteContent("Call '/wait/[number]' to have to server wait [number] seconds before replying.");
            }

            return;
        }

        public override bool Match(HttpContext context)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(context.Request.RawUrl, WaitOperation.InputRegex, RegexOptions.IgnoreCase);
        }
    }
}
