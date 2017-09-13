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

            ss.AppendLine("REQUEST:");
            ss.AppendFormat("{0} {1} HTTP{2}/{3}{4}", context.Request.HttpMethod, context.Request.RawUrl, (context.Request.IsSecureConnection ? "s" : ""), context.Request.ProtocolVersion, Environment.NewLine);

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
                ss.AppendLine("QUERY");
                ss.AppendLine(context.Request.Url.Query);
            }

            this.logger.Log(EventType.OperationInformation, "HTTP header of operation '{0}':", this.ID);
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
