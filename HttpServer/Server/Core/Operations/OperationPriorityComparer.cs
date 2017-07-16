using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Batzill.Server.Core
{
    public class OperationPriorityComparer : IComparer<Operation>
    {
        public int Compare(Operation x, Operation y)
        {
            if (x.Priority == y.Priority)
            {
                return x.Name.CompareTo(y.Name);
            }
            else
            {
                return x.Priority < y.Priority ? 1 : -1;
            }
        }
    }
}
