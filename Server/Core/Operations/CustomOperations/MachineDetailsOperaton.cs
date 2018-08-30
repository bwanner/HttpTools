using System;
using Batzill.Server.Core.Logging;
using Batzill.Server.Core.ObjectModel;
using Batzill.Server.Core.Settings;
using System.Text.RegularExpressions;
using Batzill.Server.Core.Settings.Custom.Operations;

namespace Batzill.Server.Core.Operations
{
    public class MachineDetailsOperaton : Operation
    {
        public override string Name => "Machine";

        public MachineDetailsOperaton() : base()
        {
        }

        //public override void InitializeClass(OperationSettings settings)
        //{
        //    if (!(settings is IdOperationSettings))
        //    {
        //        throw new ArgumentException($"Type '{settings.GetType()}' is invalid for this operation.");
        //    }
        //}

        public override void Execute(HttpContext context)
        {
            context.Response.SetDefaultValues();

            // Create response content


            this.logger?.Log(EventType.OperationInformation, "Check if request is authenticated");

            if(!AuthenticationOperation.HandleAuthentication(context))
            {
                return;
            }

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
