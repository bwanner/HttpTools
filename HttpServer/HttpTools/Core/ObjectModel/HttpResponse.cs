using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Batzill.Server.Core.ObjectModel
{
    public class HttpResponse : HttpReqRespBase
    {
        public string RedirectLocation
        {
            get
            {
                return this.GetHeaderValue(HttpResponseHeader.Location.ToString());
            }
            set
            {
                this.SetHeaderValue(HttpResponseHeader.Location.ToString(), value);
            }
        }

        public int StatusCode
        {
            get;  set;
        }

        public string StatusDescription
        {
            get;  set;
        }

        public HttpResponse(Version protocolVersion) : base(protocolVersion)
        {
        }

        public override void SetDefaultValues()
        {
            base.SetDefaultValues();

            this.StatusCode = 200;
            this.StatusDescription = "OK";
        }

        public void Redirect(string url)
        {
            this.StatusCode = 302;
            this.StatusDescription = "Redirect";
            this.RedirectLocation = StatusDescription;
        }
    }
}
