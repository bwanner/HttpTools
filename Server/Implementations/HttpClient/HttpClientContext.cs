using Batzill.Server.Core.ObjectModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Batzill.Server.Implementations.HttpClient
{
    public class HttpClientContext : HttpContext
    {
        public HttpListenerResponse internalResponse;

        public HttpClientContext(HttpListenerRequest request, HttpListenerResponse response) : base(HttpClientContext.ConvertRequest(request))
        {
            this.internalResponse = response;
        }

        protected override void SyncResponseInternal()
        {
            this.internalResponse.ContentEncoding = this.Response.ContentEncoding;
            this.internalResponse.ContentType = this.Response.ContentType;

            this.internalResponse.ContentLength64 = this.Response.ContentLength;
            this.internalResponse.SendChunked = this.Response.SendChuncked;

            this.internalResponse.Headers.Clear();
            this.internalResponse.Headers.Add(this.Response.Headers);

            this.internalResponse.Cookies.Add(this.Response.Cookies);

            this.internalResponse.KeepAlive = this.Response.KeepAlive;

            this.internalResponse.ProtocolVersion = this.Response.ProtocolVersion;
            this.internalResponse.RedirectLocation = this.Response.RedirectLocation;

            this.internalResponse.StatusCode = this.Response.StatusCode;
            this.internalResponse.StatusDescription = this.Response.StatusDescription;
        }

        protected override void FlushResponseInternal()
        {
            this.Response.Stream.Position = 0;
            this.Response.Stream.CopyTo(this.internalResponse.OutputStream);

            this.Response.Stream.Flush();
            this.Response.Stream.SetLength(0);
            this.Response.Stream.Position = 0;

            this.internalResponse.OutputStream.Flush();
        }

        protected override void CloseResponseInternal()
        {
            this.internalResponse.OutputStream.Close();
            this.internalResponse.Close();
        }

        protected override void SyncRequestInternal()
        {
            return;
        }

        protected override void FlushRequestInternal()
        {
            return;
        }

        protected override void CloseRequestInternal()
        {
            return;
        }

        private static HttpRequest ConvertRequest(HttpListenerRequest request)
        {
            return new HttpRequest(request.ProtocolVersion)
            {
                Cookies = request.Cookies,
                HasEntityBody = request.HasEntityBody,
                Headers = request.Headers,
                HttpMethod = request.HttpMethod,
                Stream = request.InputStream,
                IsSecureConnection = request.IsSecureConnection,
                LocalEndpoint = request.LocalEndPoint,
                RemoteEndpoint = request.RemoteEndPoint,
                Url = request.Url,
                UserHostName = request.UserHostName,
                ContentEncoding = request.ContentEncoding,
                ContentLength = request.ContentLength64,
                ContentType = request.ContentType,
                KeepAlive = request.KeepAlive,
                RequestTraceIdentifier = request.RequestTraceIdentifier.ToString(),
                UserAgent = request.UserAgent
            };
        }
    }
}
