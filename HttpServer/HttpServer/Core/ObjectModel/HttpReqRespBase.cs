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
            get
            {
                var value = GetHeaderValue(HttpRequestHeader.ContentEncoding.ToString());
                return value == null ? null : Encoding.GetEncoding(value);
            }
            set
            {
                this.SetHeaderValue(HttpRequestHeader.ContentEncoding.ToString(), value.WebName.ToString());
            }
        }

        public long? ContentLength
        {
            get
            {
                var value = GetHeaderValue(HttpRequestHeader.ContentLength.ToString());
                return value == null ? (long?)null : Convert.ToInt64(value);
            }
            set
            {
                this.SetHeaderValue(HttpRequestHeader.ContentLength.ToString(), value.ToString());
            }
        }

        public string ContentType
        {
            get
            {
                return this.GetHeaderValue(HttpRequestHeader.ContentType.ToString());
            }
            set
            {
                this.SetHeaderValue(HttpRequestHeader.ContentType.ToString(), value);
            }
        }

        public CookieCollection Cookies
        {
            get; set;
        }

        public NameValueCollection Headers
        {
            get; set;
        }

        public bool? KeepAlive
        {
            get
            {
                return string.Equals("keep-alive", GetHeaderValue(HttpRequestHeader.Connection.ToString()), StringComparison.InvariantCultureIgnoreCase);
            }
        }

        public Stream Stream;

        public Version ProtocolVersion
        {
            get; set;
        }


        public HttpReqRespBase()
        {
            this.Cookies = new CookieCollection();
            this.Headers = new NameValueCollection();
            this.Stream = new MemoryStream();
        }

        public virtual void SetDefaultValues()
        {
            this.ContentEncoding = Encoding.UTF8;
            this.ContentLength = 0;
            this.ContentType = "text/html";
            this.ProtocolVersion = new Version(1, 1);
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

            if (this.ContentLength != null)
            {
                this.ContentLength += data.Length;
            }

            this.Stream.Write(data, 0, data.Length);
            this.Stream.Flush();
        }
    }
}
