using System;

namespace Batzill.Server.Core.Settings
{
    public class EndPoint
    {
        public string HostName
        {
            get; set;
        }

        public Protocol Protocol
        {
            get; set;
        }

        public UInt16 Port
        {
            get; set;
        }

        public string CertificateThumbPrint
        {
            get; set;
        }

        public void Validate()
        {
            if (string.IsNullOrEmpty(this.HostName))
            {
                throw new NullReferenceException($"'{nameof(this.HostName)}' has to be specified.");
            }

            if (this.Protocol == Protocol.HTTPS)
            {
                if (string.IsNullOrEmpty(this.CertificateThumbPrint))
                {
                    throw new InvalidOperationException($"'{nameof(this.CertificateThumbPrint)}' can't be null or empty if protocol is set to '{nameof(Protocol.HTTPS)}'");
                }
            }

            // TO-DO add more validation for hostname
        }
    }
}
