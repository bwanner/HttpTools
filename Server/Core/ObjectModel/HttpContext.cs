using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Batzill.Server.Core.ObjectModel
{
    public abstract class HttpContext
    {
        public HttpRequest Request
        {
            get;
            private set;
        }

        public HttpResponse Response
        {
            get;
            private set;
        }

        public HttpContext(Version protocolVersion) : this(new HttpRequest(protocolVersion), new HttpResponse(protocolVersion)) { }

        public HttpContext(HttpRequest request) : this(request, new HttpResponse(request.ProtocolVersion)) {
        }
        public HttpContext(HttpResponse response) : this(new HttpRequest(response.ProtocolVersion), response) { }

        public HttpContext(HttpRequest request, HttpResponse response)
        {
            this.Request = request;
            this.Response = response;
        }

        public abstract void SyncResponse();
        public abstract void FlushResponse();
        public abstract void CloseResponse();

        public abstract void SyncRequest();
        public abstract void FlushRequest();
        public abstract void CloseRequest();
    }
}
