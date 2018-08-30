using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Batzill.Server.Core.Operations.CustomOperations.Authentication
{
    public class User
    {
        public Credentials Credentials
        {
            get; set;
        }

        public Session Session
        {
            get; set;
        }
    }
}
