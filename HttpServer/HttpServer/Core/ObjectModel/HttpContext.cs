using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Batzill.Server.Core.ObjectModel
{
    public class HttpContext
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

        public HttpContext() : this(new HttpRequest(), new HttpResponse()) { }

        public HttpContext(HttpRequest request) : this(request, new HttpResponse()) { }

        public HttpContext(HttpRequest request, HttpResponse response)
        {
            this.Request = request;
            this.Response = response;

            this.Sync();
        }

        /// <summary>
        /// Syncs properties from the request to the respone (ProtocolVersion, ...)
        /// </summary>
        protected void Sync()
        {
            this.Response.ProtocolVersion = this.Request.ProtocolVersion;
        }
    }
}
