using System;
using System.Text.RegularExpressions;
using Batzill.Server.Core.Authentication;
using Batzill.Server.Core.Exceptions;
using Batzill.Server.Core.Logging;
using Batzill.Server.Core.ObjectModel;

namespace Batzill.Server.Core.Operations
{
    public class StatusCodeOperation : Operation
    {
        public const string InputRegex = "^/statuscode/([0-9]*)$";

        public override string Name => "StatusCode";

        public StatusCodeOperation(Logger logger = null) : base(logger)
        {
        }

        protected override void ExecuteInternal(HttpContext context, IAuthenticationManager authManager)
        {
            context.Response.SetDefaultValues();

            this.logger?.Log(EventType.OperationInformation, "Got request to return given status code, try parsing status code passed by client.");

            Match result = Regex.Match(context.Request.RawUrl, StatusCodeOperation.InputRegex, RegexOptions.IgnoreCase);
            if (result.Success && result.Groups.Count == 2 && !string.IsNullOrEmpty(result.Groups[1].Value))
            {
                if (Int32.TryParse(result.Groups[1].Value, out int statusCode))
                {
                    if (statusCode < 100 || statusCode > 999)
                    {
                        this.logger?.Log(EventType.OperationInformation, "Client passed invalid status code '{0}'.", statusCode);

                        throw new BadRequestException("Invalid status code '{0}'.", statusCode);
                    }
                    else
                    {
                        this.logger?.Log(EventType.OperationInformation, "Returning status code '{0}' passed by client.", statusCode);
                        context.Response.StatusCode = statusCode;
                    }
                }
                else
                {
                    this.logger?.Log(EventType.OperationInformation, "Unable to parse '{0}' to a status code.", result.Groups[1].Value);

                    throw new BadRequestException("Unable to parse '{0}' to a status code.", result.Groups[1].Value);
                }
            }
            else
            {
                this.logger?.Log(EventType.OperationInformation, "No number passed, return info page.");
                context.Response.WriteContent("Call '/statuscode/[number]' (100 <= number < 600) to have to server return [number] as status code.");
            }

            return;
        }

        public override bool Match(HttpContext context)
        {
            return Regex.IsMatch(context.Request.RawUrl, StatusCodeOperation.InputRegex, RegexOptions.IgnoreCase);
        }
    }
}
