using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Batzill.Server.Core.Logging;
using Batzill.Server.Core.ObjectModel;
using Batzill.Server.Core.Settings;

namespace Batzill.Server.Core
{
    public interface IOperationFactory : ISettingsChangable
    {
        Operation CreateMatchingOperation(HttpContext context);
        void LoadOperations();
    }
}
