using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Batzill.Server.Core.Logging;
using Batzill.Server.Core.ObjectModel;
using Batzill.Server.Core.Settings;

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

            StringBuilder ss = new StringBuilder();
            ss.AppendFormat("{0} {1} HTTP{2}/{3}", context.Request.HttpMethod, context.Request.RawUrl, (context.Request.IsSecureConnection ? "s" : ""), context.Request.ProtocolVersion);
            ss.AppendLine();
            foreach (string header in context.Request.Headers.Keys)
            {
                ss.AppendLine(string.Format("{0}: {1}", header, context.Request.GetHeaderValue(header)));
            }

            this.logger.Log(EventType.OperationInformation, "HTTP header of operation '{0}':", this.ID);
            this.logger.Log(EventType.OperationInformation, ss.ToString());

            context.Response.WriteContent(ss.ToString());

            return;
        }

        public override bool Match(HttpContext context)
        {
            return true;
        }
    }
}
