using Batzill.Server.Core.Settings;
using System;
using System.Collections.Generic;

namespace Batzill.Server.Core
{
    public class OperationPriorityComparer : IComparer<Tuple<int, Operation>>
    {
        public int Compare(Tuple<int, Operation> x, Tuple<int, Operation> y)
        {
            if (x.Item1 == y.Item1)
            {
                return x.Item2.Name.CompareTo(y.Item2.Name);
            }
            else
            {
                return x.Item1 < y.Item1 ? 1 : -1;
            }
        }
    }
}
