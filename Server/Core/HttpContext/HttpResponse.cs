using System;

namespace Batzill.Server.Core.ObjectModel
{
    public class HttpResponse : HttpReqRespBase
    {
        public string RedirectLocation
        {
            get; set;
        }

        public int StatusCode
        {
            get;  set;
        }

        public string StatusDescription
        {
            get;  set;
        }

        public HttpResponse(Version protocolVersion) : base(protocolVersion)
        {
        }

        public override void SetDefaultValues()
        {
            base.SetDefaultValues();

            this.StatusCode = 200;
            this.StatusDescription = "OK";
        }

        public void Redirect(string url)
        {
            this.StatusCode = 302;
            this.StatusDescription = "Redirect";
            this.RedirectLocation = url;
        }

        public override void Reset()
        {
            base.Reset();

            this.RedirectLocation = null;
        }
    }
}
