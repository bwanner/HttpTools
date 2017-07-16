using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Batzill.Server.Core.Logging;
using Batzill.Server.Core.ObjectModel;
using Batzill.Server.Core.Settings;

namespace Batzill.Server.Core.Operations
{
    public class StatusEchoOperation : Operation
    {
        public const string InputRegex = "^/statuscode/?([0-9]*)$";

        public override int Priority
        {
            get
            {
                return 5;
            }
        }

        public override string Name
        {
            get
            {
                return "StatusEcho";
            }
        }

        public StatusEchoOperation() : base()
        {
        }

        public override void Execute(HttpContext context)
        {
            context.Response.SetDefaultValues();

            this.logger.Log(EventType.OperationInformation, "Got request to return given status code, try parsing status code passed by client.");

            Match result = Regex.Match(context.Request.RawUrl, StatusEchoOperation.InputRegex, RegexOptions.IgnoreCase);
            if (result.Success && result.Groups.Count == 2 && !string.IsNullOrEmpty(result.Groups[1].Value))
            {
                if (Int32.TryParse(result.Groups[1].Value, out int statusCode))
                {
                    if (statusCode < 100 || statusCode > 600)
                    {
                        this.logger.Log(EventType.OperationInformation, "Client passed invalid status code '{0}'.", statusCode);
                        context.Response.WriteContent(string.Format("Invalid status code '{0}'.", statusCode));
                    }
                    else
                    {
                        this.logger.Log(EventType.OperationInformation, "Returning status code '{0}' passed by client.", statusCode);
                        context.Response.StatusCode = statusCode;
                    }
                }
                else
                {
                    this.logger.Log(EventType.OperationInformation, "Unable to parse '{0}' to a status code.", result.Groups[1].Value);
                    context.Response.WriteContent(string.Format("Unable to parse '{0}' to a status code.", result.Groups[1].Value));
                }
            }
            else
            {
                this.logger.Log(EventType.OperationInformation, "No number passed, return info page.");
                context.Response.WriteContent("Call '/statuscode/[number]' (100 <= number < 600) to have to server return [number] as status code.");
            }


            return;
        }

        public override bool Match(HttpContext context)
        {
            return Regex.IsMatch(context.Request.RawUrl, StatusEchoOperation.InputRegex, RegexOptions.IgnoreCase);
        }
    }
}
