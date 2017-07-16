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
                this.SendChuncked = false;
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

        public bool KeepAlive
        {
            get
            {
                string value = GetHeaderValue(HttpRequestHeader.Connection.ToString());
                if (this.ProtocolVersion.Minor == 1 && string.IsNullOrEmpty(value))
                {
                    return true;
                }

                return string.Equals("keep-alive", value, StringComparison.InvariantCultureIgnoreCase);
            }
            set
            {
                if (value)
                {
                    this.SetHeaderValue(HttpRequestHeader.Connection.ToString(), "keep-alive");
                }
                else
                {
                    this.SetHeaderValue(HttpRequestHeader.Connection.ToString(), "close");
                }
            }
        }

        public bool SendChuncked
        {
            get
            {
                return string.Equals("chunked", GetHeaderValue(HttpResponseHeader.TransferEncoding.ToString()), StringComparison.InvariantCultureIgnoreCase);
            }
            set
            {
                if (value)
                {
                    this.SetHeaderValue(HttpResponseHeader.TransferEncoding.ToString(), "chunked");
                    this.Headers.Remove(HttpResponseHeader.ContentLength.ToString());
                }
                else if (this.SendChuncked)
                {
                    this.Headers.Remove(HttpResponseHeader.TransferEncoding.ToString());
                }
            }
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

            if (this.ContentLength != null)
            {
                this.ContentLength += data.Length;
            }

            this.Stream.Write(data, 0, data.Length);
        }
    }
}
