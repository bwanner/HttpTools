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
            get; set;
        }

        public string UserHostName
        {
            get; set;
        }

        public HttpRequest(Version protocolVersion) : base(protocolVersion)
        {
        }

        public override void SetDefaultValues()
        {
            base.SetDefaultValues();

            HasEntityBody = false;
            HttpMethod = "GET";
            IsSecureConnection = false;
        }

        public override void Reset()
        {
            base.Reset();

            this.LocalEndpoint = null;
            this.RemoteEndpoint = null;
            this.Url = null;
            this.UserAgent = null;
            this.UserHostName = null;
        }
    }
}
