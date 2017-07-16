using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Batzill.Server.Core.IO
{
    public class SystemFileReader : IFileReader
    {
        private FileStream fileStream;

        public SystemFileReader(string file = "", bool lockFile = false)
        {
            if (!string.IsNullOrEmpty(file))
            {
               this.Open(file, lockFile);
            }
        }

        public IDisposable Open(string file, bool lockFile = false)
        {
            if (lockFile)
            {
                this.fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            else
            {
                this.fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
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

        public IEnumerable<char> ReadCharByChar()
        {
            using (StreamReader reader = new StreamReader(this.fileStream))
            {
                while (!reader.EndOfStream)
                {
                    yield return (char)reader.Read();
                }
            }
        }

        public IEnumerable<string> ReadLineByLine()
        {
            using (StreamReader reader = new StreamReader(this.fileStream))
            {
                while (!reader.EndOfStream)
                {
                    yield return reader.ReadLine();
                }
            }
        }

        public string ReadEntireFile()
        {
            using (StreamReader reader = new StreamReader(this.fileStream))
            {
                return reader.ReadToEnd();
            }
        }

        public void Dispose()
        {
            this.Close();
        }
    }
}
