using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Batzill.Server.Core.Logging;
using Batzill.Server.Core.ObjectModel;
using Batzill.Server.Core.Settings;
using System.Net;

namespace Batzill.Server.Core.Operations
{
    public class EchoOperation : Operation
    {

        public override int Priority
        {
            get
            {
                return 0;
            }
        }

        public override string Name
        {
            get
            {
                return "Echo";
            }
        }

        public EchoOperation() : base()
        {
        }

        public override void Execute(HttpContext context)
        {
            context.Response.SetDefaultValues();

            // Create response content
            StringBuilder ss = new StringBuilder();

            ss.AppendLine("ENDPOINT INFORMATION:");
            ss.AppendFormat("IP: '{0}'{1}", context.Request.LocalEndpoint.Address, Environment.NewLine);
            ss.AppendFormat("PORT: '{0}'{1}", context.Request.LocalEndpoint.Port, Environment.NewLine);
            ss.AppendFormat("HOST: '{0}'{1}", context.Request.UserHostName, Environment.NewLine);


            if (context.Request.IsSecureConnection)
            {
                ss.AppendLine();
                ss.AppendLine("SECURE CONNECTION");
            }

            ss.AppendLine();
            ss.AppendLine("REQUEST:");
            ss.AppendFormat("{0} {1} HTTP/{2}{3}", context.Request.HttpMethod, context.Request.RawUrl, context.Request.ProtocolVersion, Environment.NewLine);

            if (context.Request.Headers.Keys.Count > 0)
            {
                ss.AppendLine();
                ss.AppendLine("HEADER:");
                foreach (string header in context.Request.Headers.Keys)
                {
                    ss.AppendLine(string.Format("{0}: {1}", header, context.Request.GetHeaderValue(header)));
                }
            }

            if(!string.IsNullOrEmpty(context.Request.Url.Query))
            {
                ss.AppendLine();
                ss.AppendLine("QUERY:");

                var parameters = System.Web.HttpUtility.ParseQueryString(context.Request.Url.Query);
                foreach (string key in parameters.Keys)
                {
                    foreach (string value in parameters.GetValues(key))
                    {
                        ss.AppendLine(string.Format("{0} = {1}", key, value));
                    }
                }
            }
            
            ss.AppendLine();
            ss.AppendLine("TIMESTAMP:");
            ss.AppendLine($"{DateTime.UtcNow.ToLongDateString()} {DateTime.UtcNow.ToLongTimeString()} (UTC)");

            this.logger.Log(EventType.OperationInformation, "HTTP Details for operation '{0}':", this.ID);
            this.logger.Log(EventType.OperationInformation, ss.ToString());

            context.Response.WriteContent(ss.ToString());
            context.SyncResponse();

            return;
        }

        public override bool Match(HttpContext context)
        {
            return true;
        }
    }
}
