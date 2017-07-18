using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Batzill.Server.Core.Logging;
using Batzill.Server.Core.ObjectModel;
using Batzill.Server.Core.Settings;

namespace Batzill.Server.Core.Operations
{
    public class DownloadOperation : Operation
    {
        private const string InputRegex = "^/download/?([0-9]*)$";
        private static Random Random = new Random((int)(DateTime.Now.Ticks % int.MaxValue));

        public override int Priority
        {
            get
            {
                return 7;
            }
        }

        public override string Name
        {
            get
            {
                return "Download";
            }
        }

        public DownloadOperation() : base()
        {
        }

        public override void Execute(HttpContext context)
        {
            context.Response.SetDefaultValues();

            this.logger.Log(EventType.OperationInformation, "Got request to download data, try parsing size passed by client.");

            Match result = Regex.Match(context.Request.RawUrl, DownloadOperation.InputRegex, RegexOptions.IgnoreCase);
            if (result.Success && result.Groups.Count == 2 && !string.IsNullOrEmpty(result.Groups[1].Value))
            {
                if (Int64.TryParse(result.Groups[1].Value, out long bytes))
                {
                    if (bytes > 10000000000)
                    {
                        context.Response.WriteContent(string.Format("File is to big '{0}', only supported up to 10GB.", bytes));
                        return;
                    }

                    this.logger.Log(EventType.OperationInformation, "Will file of size {0}B.", bytes);
                    
                    context.Response.SetHeaderValue("Content-Type", "application /octet-stream");
                    context.Response.SetHeaderValue("Content-Disposition", String.Format("attachment; filename=\"TestFile_{0}B.file\"", bytes));
                    context.Response.ContentLength = bytes;

                    long bufferSize = 1024, bytesSend = 0;
                    byte[] data = new byte[bufferSize];
                    while (bytesSend < bytes)
                    {
                        Random.NextBytes(data);
                        context.Response.Stream.Write(data, 0, (int)Math.Min(bufferSize, bytes - bytesSend));
                        context.FlushResponse();
                        bytesSend += bufferSize;
                    }
                }
                else
                {
                    this.logger.Log(EventType.OperationInformation, "Unable to parse '{0}' to long.", result.Groups[1].Value);

                    context.Response.WriteContent(string.Format("Unable to parse '{0}' to long.", result.Groups[1].Value));
                }
            }
            else
            {
                this.logger.Log(EventType.OperationInformation, "No number passed, return info page.");

                context.Response.WriteContent("Call '/download/[number]' to have the server return [number] Bytes of data. (Max 10GB)");
            }

            return;
        }

        public override bool Match(HttpContext context)
        {
            return Regex.IsMatch(context.Request.RawUrl, DownloadOperation.InputRegex, RegexOptions.IgnoreCase);
        }
    }
}
