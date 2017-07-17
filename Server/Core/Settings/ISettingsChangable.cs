using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Batzill.Server.Core.Settings
{
    public interface ISettingsChangable
    {
        bool ApplySettings(HttpServerSettings settings);
    }
}
