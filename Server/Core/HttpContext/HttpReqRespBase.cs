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
    public class HttpReqRespBase
    {
        public Encoding ContentEncoding
        {
            get; set;
        }

        public long ContentLength
        {
            get; set;
        }

        public string ContentType
        {
            get; set;
        }

        public CookieCollection Cookies
        {
            get; set;
        }

        public NameValueCollection Headers
        {
            get; set;
        }

        public bool KeepAlive
        {
            get; set;
        }

        public bool SendChuncked
        {
            get; set;
        }

        public Stream Stream
        {
            get; set;
        }

        public Version ProtocolVersion
        {
            get;
            private set;
        }


        public HttpReqRespBase(Version ProtocolVersion)
        {
            this.ProtocolVersion = ProtocolVersion;
            this.Cookies = new CookieCollection();
            this.Headers = new NameValueCollection();
            this.Stream = new MemoryStream();
        }

        public virtual void SetDefaultValues()
        {
            this.ContentEncoding = Encoding.UTF8;
            this.ContentLength = 0;
            this.ContentType = "text/html";
        }

        public string GetHeaderValue(string header, string def = null)
        {
            if (string.IsNullOrEmpty(header))
            {
                throw new ArgumentNullException("Please specify a header name", "header");
            }

            string headerValue = this.Headers[header];

            return headerValue ?? def;
        }

        public void SetHeaderValue(string header, string value)
        {
            if (string.IsNullOrEmpty(header))
            {
                throw new ArgumentNullException("Please specify a header name", "header");
            }

            if (value != null)
            {
                this.Headers[header] = value;
            }
        }

        public void WriteContent(string value, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = this.ContentEncoding ?? Encoding.UTF8;
            }

            byte[] data = encoding.GetBytes(value);

            this.ContentLength += data.Length;
            this.Stream.Write(data, 0, data.Length);
        }
    }
}
