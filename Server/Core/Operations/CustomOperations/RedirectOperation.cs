﻿using System.Text.RegularExpressions;
using Batzill.Server.Core.Authentication;
using Batzill.Server.Core.Logging;
using Batzill.Server.Core.ObjectModel;

namespace Batzill.Server.Core.Operations
{
    public class RedirectOperation : Operation
    {
        private const string InputRegex = "^/redirect/?(.*)$";

        public override string Name => "Redirect";

        public RedirectOperation(Logger logger = null) : base(logger)
        {
        }

        protected override void ExecuteInternal(HttpContext context, IAuthenticationManager authManager)
        {
            context.Response.SetDefaultValues();

            this.logger?.Log(EventType.OperationInformation, "Got request to redirect request, try parsing redirect target passed by client.");

            Match result = Regex.Match(context.Request.RawUrl, RedirectOperation.InputRegex, RegexOptions.IgnoreCase);
            if (result.Success && result.Groups.Count == 2 && !string.IsNullOrEmpty(result.Groups[1].Value))
            {
                context.Response.Redirect(string.Format("/{0}", result.Groups[1].Value));
            }
            else
            {
                this.logger?.Log(EventType.OperationInformation, "No redirect target passed, return info page.");

                context.Response.WriteContent("Call '/redirect/[path]' to have to server redirect the call to [path].");
            }

            return;
        }

        public override bool Match(HttpContext context)
        {
            return Regex.IsMatch(context.Request.RawUrl, RedirectOperation.InputRegex, RegexOptions.IgnoreCase);
        }
    }
}
