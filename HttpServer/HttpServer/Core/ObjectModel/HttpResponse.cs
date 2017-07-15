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
    public class HttpResponse
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

        public NameValueCollection Headers
        {
            get; private set;
        }

        public string KeepAlive
        {
            get; private set;
        }

        public StreamWriter OutputStream
        {
            get; private set;
        }

        public Version ProtocolVersion
        {
            get; private set;
        }

        public string RedirectLocation
        {
            get; private set;
        }

        public int StatusCode
        {
            get; private set;
        }

        public string StatusDescription
        {
            get; private set;
        }

        public HttpResponse()
        {
            this.Cookies = new CookieCollection();
            this.Headers = new NameValueCollection();
        }

        public void Redirect(string url)
        {
            this.StatusCode = 302;
            this.StatusDescription = "Redirect";
            this.RedirectLocation = StatusDescription;
        }
    }
}
