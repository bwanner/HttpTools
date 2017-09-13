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
        /// <summary>
        /// Group[2]: size
        /// Group[3]: unit
        /// Group[6]: chunksize
        /// </summary>
        private const string InputRegex = @"^\/download\/?(([0-9]+)(b|kb|mb|gb|)?)?(\?chunked(=([0-9]+))?)?$";
        private static Random Random = new Random((int)(DateTime.Now.Ticks % int.MaxValue));

        private const int DefaultChunkSize = 8192;

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

            Match result = Regex.Match(context.Request.Url.PathAndQuery, DownloadOperation.InputRegex, RegexOptions.IgnoreCase);
            if (result.Success && !string.IsNullOrEmpty(result.Groups[1].Value))
            {
                if (Int64.TryParse(result.Groups[2].Value, out long inputNumber))
                {
                    string requestedUnit = string.IsNullOrEmpty(result.Groups[3].Value) ? "b" : result.Groups[3].Value;
                    long requestedDownloadSize = inputNumber;
                    switch(requestedUnit.ToLowerInvariant())
                    {
                        case "b":
                            break;
                        case "kb":
                            requestedDownloadSize *= 1 << 10;
                            break;
                        case "mb":
                            requestedDownloadSize *= 1 << 20;
                            break;
                        case "gb":
                            requestedDownloadSize *= 1 << 30;
                            break;
                        default:
                            this.logger.Log(EventType.OperationError, "Unknown size unit '{0}'", requestedUnit);
                            context.Response.WriteContent(string.Format("Unknown size unit '{0}'", requestedUnit));
                            return;

                    }

                    if (requestedDownloadSize > 34359738368) // = 32g
                    {
                        this.logger.Log(EventType.OperationError, "File is to big '{0}'{1}, only supported up to 32GB.", inputNumber, requestedUnit);
                        context.Response.WriteContent(string.Format("File is to big '{0}'{1}, only supported up to 32GB.", inputNumber, requestedUnit));
                        return;
                    }

                    bool chunked = !string.IsNullOrEmpty(result.Groups[4].Value);
                    long chunkSize = DownloadOperation.DefaultChunkSize;

                    if (chunked && !string.IsNullOrEmpty(result.Groups[6].Value))
                    {
                        if(!Int64.TryParse(result.Groups[6].Value, out chunkSize) || chunkSize < 1)
                        {
                            this.logger.Log(EventType.OperationError, "Unable to parse chunk size '{0}' to long.", result.Groups[1].Value);
                            context.Response.WriteContent(string.Format("Unable to parse chunk size '{0}' to long.", result.Groups[1].Value));

                            return;
                        }

                        chunkSize = Math.Min(chunkSize, requestedDownloadSize);
                    }

                    this.logger.Log(EventType.OperationInformation, "Will return file of size {0}{1} ({2}).", inputNumber, requestedUnit, chunked ? "chunked" : "one part");

                    context.Response.SetHeaderValue("Content-Type", "application/octet-stream");
                    context.Response.SetHeaderValue("Content-Disposition", String.Format("attachment;filename=\"TestFile_{0}{1}.file\"", inputNumber, requestedUnit));

                    if (!chunked)
                    {
                        context.Response.ContentLength = requestedDownloadSize;
                    }
                    else
                    {
                        context.Response.SendChuncked = true;
                    }

                    // sync response before sending
                    context.SyncResponse();

                    long bufferSize = chunkSize, bytesSend = 0;
                    byte[] data = new byte[bufferSize];
                    while (bytesSend < requestedDownloadSize)
                    {
                        Random.NextBytes(data);
                        context.Response.Stream.Write(data, 0, (int)Math.Min(bufferSize, requestedDownloadSize - bytesSend));
                        context.FlushResponse();
                        bytesSend += bufferSize;
                    }
                }
                else
                {
                    this.logger.Log(EventType.OperationError, "Unable to parse '{0}' to long.", result.Groups[1].Value);
                    context.Response.WriteContent(string.Format("Unable to parse '{0}' to long.", result.Groups[1].Value));
                }
            }
            else
            {
                this.logger.Log(EventType.OperationInformation, "No number passed, return info page.");

                context.Response.WriteContent("Call '/download/[number](unit)' to have the server return [number] (unit=b/kb/mb/gb, default b) of data. (Max 32GB)");
            }
        }

        public override bool Match(HttpContext context)
        {
            return Regex.IsMatch(context.Request.RawUrl, DownloadOperation.InputRegex, RegexOptions.IgnoreCase);
        }
    }
}
