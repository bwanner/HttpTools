using System;
using System.Text.RegularExpressions;
using Batzill.Server.Core.Logging;
using Batzill.Server.Core.ObjectModel;
using Batzill.Server.Core.Authentication;

namespace Batzill.Server.Core.Operations
{
    public class MachineDetailsOperaton : Operation
    {
        public override string Name => "Machine";

        public MachineDetailsOperaton(Logger logger = null) : base(logger)
        {
        }

        protected override void ExecuteInternal(HttpContext context, IAuthenticationManager authManager)
        {
            context.Response.SetDefaultValues();

            // Create response content


            this.logger?.Log(EventType.OperationInformation, "Check if request is authenticated");

            this.logger?.Log(EventType.OperationInformation, "Gathering all machine details ...");

            context.Response.WriteContent(string.Format("Machine Details:\r\n"));
            context.Response.WriteContent(string.Format("   Name: {0}\r\n", Environment.MachineName));

            context.SyncResponse();

            return;
        }

        public override bool Match(HttpContext context)
        {
            return Regex.IsMatch(context.Request.RawUrl, "^/machine$", RegexOptions.IgnoreCase);
        }
    }
}
