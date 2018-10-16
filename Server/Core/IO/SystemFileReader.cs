using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

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

        public void Dispose()
        {
            this.Close();
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

        public IEnumerable<char> StreamCharByChar(bool skipExisting = true, int idleTimeout = 1000, int sleepTime = 100)
        {
            using (StreamReader reader = new StreamReader(this.fileStream))
            {
                long idleTime = 0;

                if (skipExisting)
                {
                    reader.BaseStream.Seek(0, SeekOrigin.End);
                }

                do
                {
                    while (!reader.EndOfStream)
                    {
                        yield return (char)reader.Read();
                        idleTime = 0;
                    }

                    Thread.Sleep(sleepTime);
                    idleTime += sleepTime;
                    
                } while (idleTime < idleTimeout);
            }
        }

        public IEnumerable<string> StreamLineByLine(bool skipExisting = true, int idleTimeout = 1000, int sleepTime = 100)
        {
            using (StreamReader reader = new StreamReader(this.fileStream))
            {
                long idleTime = 0;

                if (skipExisting)
                {
                    reader.BaseStream.Seek(0, SeekOrigin.End);
                }

                do
                {
                    while (!reader.EndOfStream)
                    {
                        yield return reader.ReadLine();
                        idleTime = 0;
                    }

                    Thread.Sleep(sleepTime);
                    idleTime += sleepTime;

                } while (idleTime < idleTimeout);
            }
        }
    }
}
