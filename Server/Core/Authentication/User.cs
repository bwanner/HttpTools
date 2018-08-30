using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Batzill.Server.Core.Authentication
{
    public class User
    {
        public string Id
        {
            get; set;
        }

        public Session Session
        {
            get; set;
        }
    }
}
