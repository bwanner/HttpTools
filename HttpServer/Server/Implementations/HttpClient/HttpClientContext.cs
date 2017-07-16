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
                Url = request.Url
            };
        }

        public override void FlushResponse()
        {
            this.Response.Stream.Position = 0;
            this.Response.Stream.CopyTo(this.internalResponse.OutputStream);

            this.Response.Stream.Flush();
            this.Response.Stream.SetLength(0);
            this.Response.Stream.Position = 0;

            this.internalResponse.OutputStream.Flush();
        }

        public override void SyncResponse()
        {
            this.internalResponse.ContentEncoding = this.Response.ContentEncoding;

            if (this.Response.ContentLength.HasValue)
            {
                this.internalResponse.ContentLength64 = this.Response.ContentLength.Value;
            }

            this.internalResponse.SendChunked = this.Response.SendChuncked;
            this.internalResponse.ContentType = this.Response.ContentType;
            this.internalResponse.Cookies = this.Response.Cookies;

            this.internalResponse.Headers.Clear();
            this.internalResponse.Headers.Add(this.Response.Headers);

            if (this.Response.KeepAlive)
            {
                this.internalResponse.KeepAlive = this.Response.KeepAlive;
            }

            this.internalResponse.ProtocolVersion = this.Response.ProtocolVersion;
            this.internalResponse.RedirectLocation = this.Response.RedirectLocation;
            this.internalResponse.StatusCode = this.Response.StatusCode;
            this.internalResponse.StatusDescription = this.Response.StatusDescription;
        }



        public override void FlushRequest()
        {
            return;
        }

        public override void SyncRequest()
        {
            return;
        }
    }
}
