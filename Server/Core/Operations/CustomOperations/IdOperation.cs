using System;
using System.Text.RegularExpressions;
using Batzill.Server.Core.Logging;
using Batzill.Server.Core.ObjectModel;
using Batzill.Server.Core.Settings;
using Batzill.Server.Core.Settings.Custom.Operations;
using Batzill.Server.Core.Authentication;

namespace Batzill.Server.Core.Operations
{
    public class IdOperation : Operation
    {
        private static string ServerId;

        public override string Name => "Id";

        public IdOperation(Logger logger = null) : base(logger)
        {
        }

        protected override void InitializeClassInternal(OperationSettings settings, IAuthenticationManager authManager)
        {
            if (!(settings is IdOperationSettings))
            {
                throw new ArgumentException($"Type '{settings.GetType()}' is invalid for this operation.");
            }

            IdOperation.ServerId = (settings as IdOperationSettings).ServerId;
        }


        protected override void ExecuteInternal(HttpContext context, IAuthenticationManager authManager)
        {
            context.Response.SetDefaultValues();

            // Create response content

            this.logger?.Log(EventType.OperationInformation, "Returning the Id of the server: '{0}'.", IdOperation.ServerId);

            context.Response.WriteContent(IdOperation.ServerId);
            context.SyncResponse();

            return;
        }

        public override bool Match(HttpContext context)
        {
            return Regex.IsMatch(context.Request.RawUrl, "^/id$", RegexOptions.IgnoreCase);
        }
    }
}
