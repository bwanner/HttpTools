using System;
using System.IO;

namespace Batzill.Server.Core.IO
{
    public class SystemFileWriter : IFileWriter
    {
        private FileStream fileStream;

        public IDisposable Open(string file, bool lockFile = false)
        {
            if (lockFile)
            {
                this.fileStream = new FileStream(file, FileMode.Append, FileAccess.Write, FileShare.Read);
            }
            else
            {
                this.fileStream = new FileStream(file, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            }

            return this.fileStream;
        }

        public void Close()
        {
            if (this.fileStream != null)
            {
                this.fileStream.Close();
                this.fileStream.Dispose();
            }
        }

        public void Write(string text)
        {
            using (StreamWriter writer = new StreamWriter(this.fileStream))
            {
                writer.Write(text);
            }
        }

        public void WriteLine(string text)
        {
            using (StreamWriter writer = new StreamWriter(this.fileStream))
            {
                writer.WriteLine(text);
            }
        }

        public void Dispose()
        {
            this.Close();
        }
    }
}
