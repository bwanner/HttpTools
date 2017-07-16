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
            return new HttpRequest()
            {
                Cookies = request.Cookies,
                HasEntityBody = request.HasEntityBody,
                Headers = request.Headers,
                HttpMethod = request.HttpMethod,
                Stream = request.InputStream,
                IsSecureConnection = request.IsSecureConnection,
                LocalEndpoint = request.LocalEndPoint,
                ProtocolVersion = request.ProtocolVersion,
                RemoteEndpoint = request.RemoteEndPoint,
                Url = request.Url
            };
        }

        public void SyncResponse()
        {
            this.internalResponse.ContentEncoding = this.Response.ContentEncoding;

            if (this.Response.ContentLength.HasValue)
            {
                this.internalResponse.ContentLength64 = this.Response.ContentLength.Value;
            }

            this.internalResponse.ContentType = this.Response.ContentType;
            this.internalResponse.Cookies = this.Response.Cookies;

            this.internalResponse.Headers.Clear();
            this.internalResponse.Headers.Add(this.Response.Headers);

            if (this.Response.KeepAlive.HasValue)
            {
                this.internalResponse.KeepAlive = this.Response.KeepAlive.Value;
            }

            this.internalResponse.ProtocolVersion = this.Response.ProtocolVersion;
            this.internalResponse.RedirectLocation = this.Response.RedirectLocation;
            this.internalResponse.StatusCode = this.Response.StatusCode;
            this.internalResponse.StatusDescription = this.Response.StatusDescription;

            this.Response.Stream.Position = 0;
            this.Response.Stream.CopyTo(this.internalResponse.OutputStream);
        }
    }
}
