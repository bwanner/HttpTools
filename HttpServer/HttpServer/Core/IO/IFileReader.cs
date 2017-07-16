using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Batzill.Server.Core.IO
{
    public interface IFileReader : IDisposable
    {
        IDisposable Open(string file, bool lockFile = false);
        void Close();

        IEnumerable<char> ReadCharByChar();
        IEnumerable<string> ReadLineByLine();
        string ReadEntireFile();
    }
}
