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
    public class HttpRequest : HttpReqRespBase
    {
        public bool HasEntityBody
        {
            get; set;
        }

        public string HttpMethod
        {
            get; set;
        }

        public bool IsSecureConnection
        {
            get; set;
        }

        public IPEndPoint LocalEndpoint
        {
            get; set;
        }

        public string QueryString
        {
            get
            {
                return this.Url == null ? "" : this.Url.Query;
            }
        }

        public string RawUrl
        {
            get
            {
                return this.Url == null ? "" : this.Url.AbsolutePath;
            }
        }

        public IPEndPoint RemoteEndpoint
        {
            get; set;
        }

        public string RequestTraceIdentifier
        {
            get
            {
                return this.GetHeaderValue("X-Request-ID");
            }
            set
            {
                this.SetHeaderValue("X-Request-ID", value);
            }
        }

        public Uri Url
        {
            get; set;
        }

        public string UserAgent
        {
            get
            {
                return this.GetHeaderValue(HttpRequestHeader.UserAgent.ToString());
            }
            set
            {
                this.SetHeaderValue(HttpRequestHeader.UserAgent.ToString(), value);
            }
        }

        public string UserHostName
        {
            get
            {
                return this.GetHeaderValue(HttpRequestHeader.Host.ToString());
            }
            set
            {
                this.SetHeaderValue(HttpRequestHeader.Host.ToString(), value);
            }
        }

        public HttpRequest() : base()
        {
        }

        public override void SetDefaultValues()
        {
            base.SetDefaultValues();

            HasEntityBody = false;
            HttpMethod = "GET";
            IsSecureConnection = false;
        }
    }
}
