using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Batzill.Server.Core.Logging;
using Batzill.Server.Core.ObjectModel;
using Batzill.Server.Core.Settings;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections.Specialized;

namespace Batzill.Server.Core.Operations
{
    public class SetCookieOperation : Operation
    {
        private const string InputRegex = "^/SetCookie$";

        public override int Priority
        {
            get
            {
                return 8;
            }
        }

        public override string Name
        {
            get
            {
                return "SetCookie";
            }
        }

        public SetCookieOperation() : base()
        {
        }

        public override void Execute(HttpContext context)
        {
            context.Response.SetDefaultValues();
            
            StringBuilder response = new StringBuilder();
            NameValueCollection requestedCookies = System.Web.HttpUtility.ParseQueryString(context.Request.Url.Query);

            this.logger.Log(EventType.OperationInformation, "Found {0} Set-Cookie requests", requestedCookies.Count);
            response.AppendFormat("Found {0} Set-Cookie requests:{1}", requestedCookies.Count, Environment.NewLine);

            foreach (string key in requestedCookies)
            {
                string value = requestedCookies[key];

                this.logger.Log(EventType.OperationInformation, "Set cookie \"{0}\" to \"{1}\"", key, value);
                response.AppendFormat("Set cookie \"{0}\" to \"{1}\"", key, value); // output format might change

                context.Response.Headers.Add("Set-Cookie", string.Format("{0}={1}", key, value));
            }
            
            context.Response.WriteContent(response.ToString());
            context.SyncResponse();

            return;
        }

        public override bool Match(HttpContext context)
        {
            return Regex.IsMatch(context.Request.RawUrl, SetCookieOperation.InputRegex, RegexOptions.IgnoreCase);
        }
    }
}
