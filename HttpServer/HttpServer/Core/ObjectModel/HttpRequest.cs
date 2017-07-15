using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer.Core.ObjectModel
{
    public class HttpRequest
    {
        public Encoding ContentEncoding
        {
            get; private set;
        }

        public long ContentLength
        {
            get; private set;
        }

        public string ContentType
        {
            get; private set;
        }

        public CookieCollection Cookies
        {
            get; private set;
        }

        public bool HasEntityBody
        {
            get; private set;
        }

        public NameValueCollection Headers
        {
            get; private set;
        }

        public string HostAddress
        {
            get; private set;
        }

        public string HostName
        {
            get; private set;
        }

        public string HttpMethod
        {
            get; private set;
        }

        public Version ProtocolVersion
        {
            get; private set;
        }

        public StreamReader InputStream
        {
            get; private set;
        }

        public bool IsSecureConnection
        {
            get; private set;
        }

        public string KeepAlive
        {
            get; private set;
        }

        public IPEndPoint LocalEndpoint
        {
            get; private set;
        }

        public string QueryString
        {
            get; private set;
        }

        public string RawUrl
        {
            get; private set;
        }

        public IPEndPoint RemoteEndpoint
        {
            get; private set;
        }

        public Guid RequestTraceIdentifier
        {
            get; private set;
        }

        public Uri Url
        {
            get; private set;
        }

        public string UserAgent
        {
            get; private set;
        }

        public HttpRequest()
        {
            this.Cookies = new CookieCollection();
            this.Headers = new NameValueCollection();
        }
    }
}
