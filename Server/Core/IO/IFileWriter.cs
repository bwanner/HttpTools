using System;

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
