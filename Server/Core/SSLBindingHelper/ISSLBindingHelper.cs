using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Batzill.Server.Core.SSLBindingHelper
{
    public interface ISSLBindingHelper
    {
        string DefaultEndpointHost
        {
            get;
        }
        bool TryAddOrUpdateCertBinding(string certThumbprint, string appId, string port, string host);
        bool TryAddCertBinding(string certThumbprint, string appId, string port, string host);
        bool TryGetExistingBinding(out string certThumbprint, out string appId, string port, string host);
        bool TryDeleteExistingBinding(string port, string host);
    }
}
