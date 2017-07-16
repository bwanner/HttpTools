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


        IEnumerable<char> StreamCharByChar(bool skipExisting = true, int idleTimeout = 1000, int sleepTime = 100);
        IEnumerable<string> StreamLineByLine(bool skipExisting = true, int idleTimeout = 1000, int sleepTime = 100);
    }
}
