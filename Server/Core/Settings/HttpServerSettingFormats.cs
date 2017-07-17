using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Batzill.Server.Core.Settings
{
    public static class HttpServerSettingFormats
    {
        /// <summary>
        /// Group[1]: Endpoint
        /// Group[2]: Protocol
        /// Group[3]: Host
        /// Group[5]: Port
        /// Group[6]: Path
        /// Group[9]: Cert thumbprint
        /// </summary>
        public const string EndpointFormat = @"^((https?)\:\/\/([a-zA-Z0-9\.\-]+|\+|\*)(\:([0-9]+))?(\/([0-9a-z-A-Z]+\/)*))( ([0-9a-fA-F]*))?$";
    }
}
