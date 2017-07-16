using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Batzill.Server.Core.IO
{
    public interface IFileWriter : IDisposable
    {
        IDisposable Open(string file, bool lockFile = false);
        void Close();

        void Write(string text);
        void WriteLine(string text);
    }
}
