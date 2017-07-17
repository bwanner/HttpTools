﻿using System;
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
    public class RedirectOperation : Operation
    {
        private const string InputRegex = "^/redirect/?(.*)$";

        public override int Priority
        {
            get
            {
                return 6;
            }
        }

        public override string Name
        {
            get
            {
                return "Redirect";
            }
        }

        public RedirectOperation() : base()
        {
        }

        public override void Execute(HttpContext context)
        {
            context.Response.SetDefaultValues();

            this.logger.Log(EventType.OperationInformation, "Got request to redirect request, try parsing redirect target passed by client.");

            Match result = Regex.Match(context.Request.RawUrl, RedirectOperation.InputRegex, RegexOptions.IgnoreCase);
            if (result.Success && result.Groups.Count == 2 && !string.IsNullOrEmpty(result.Groups[1].Value))
            {
                context.Response.Redirect(string.Format("/{0}", result.Groups[1].Value));
            }
            else
            {
                this.logger.Log(EventType.OperationInformation, "No redirect target passed, return info page.");

                context.Response.WriteContent("Call '/redirect/[address]' to have to server redirect the call to [address].");
            }

            return;
        }

        public override bool Match(HttpContext context)
        {
            return Regex.IsMatch(context.Request.RawUrl, RedirectOperation.InputRegex, RegexOptions.IgnoreCase);
        }
    }
}