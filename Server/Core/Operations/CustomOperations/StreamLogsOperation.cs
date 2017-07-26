using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Batzill.Server.Core.IO;
using Batzill.Server.Core.Logging;
using Batzill.Server.Core.ObjectModel;
using Batzill.Server.Core.Settings;
using System.Threading;

namespace Batzill.Server.Core.Operations
{
    public class StreamLogsOperation : Operation
    {
        private DateTime lastLogTime;
        private bool failed = true;

        public override int Priority
        {
            get
            {
                return 3;
            }
        }

        public override string Name
        {
            get
            {
                return "StreamLogs";
            }
        }

        public StreamLogsOperation() : base()
        {
        }

        public override void Execute(HttpContext context)
        {
            if (!(this.logger.logWriter is MultiLogWriter))
            {
                this.logger.Log(EventType.OperationError, "Unable to attach EventLogWriter (Passed logwriter is no MultiLogWriter).");
                return;
            }

            MultiLogWriter logWriter = this.logger.logWriter as MultiLogWriter;
            EventLogWriter eventWriter = new EventLogWriter();

            this.logger.Log(EventType.OperationInformation, "Set response headers.");

            context.Response.SetDefaultValues();
            context.Response.SendChuncked = true;
            context.SyncResponse();


            this.logger.Log(EventType.OperationInformation, "Setup event writer.");

            this.failed = false;
            eventWriter.LogWritten += (sender, log) =>
            {
                lock (context)
                {
                    try
                    {
                        this.lastLogTime = DateTime.Now;

                        context.Response.WriteContent(string.Format("[{0} | {1}] {2}{3}", log.Timestamp, log.EventType, string.Join(", ", log.ExtendedData), Environment.NewLine));
                        context.FlushResponse();
                    }
                    catch(Exception ex)
                    {
                        this.logger.Log(EventType.OperationError, "Failed to write log to stream, stop operation: {0}", ex.Message);
                        this.failed = true;
                    }
                }
            };


            this.logger.Log(EventType.OperationInformation, "Add event writer to global log writer.");

            try
            {
                this.lastLogTime = DateTime.Now;
                logWriter.Add(eventWriter);
            }
            catch (Exception ex)
            {
                this.logger.Log(EventType.OperationError, "Unable to attach EventLogWriter (Adding failed): {0}", ex.Message);
                return;
            }

            int idleTimeout = 300;
            int idleTime = 0;
            int sleepTime = 1000;

            do
            {
                Thread.Sleep(sleepTime);

                idleTime = (int)(DateTime.Now - this.lastLogTime).TotalSeconds;

            } while (!failed && idleTime <= idleTimeout);

            // only output this information when stream didn't stop because flushing log data to stream failed.
            if(idleTime > idleTimeout)
            {
                this.logger.Log(EventType.OperationInformation, "No log event for {0}s, stopping the log stream.", idleTimeout);
            }

            this.logger.Log(EventType.OperationInformation, "Remove event writer to global log writer.");

            try
            {
                logWriter.Remove(eventWriter);
            }
            catch (Exception ex)
            {
                this.logger.Log(EventType.SystemError, "Unable to remove EventLogWriter: {0}", ex.Message);
            }

            return;
        }

        public override bool Match(HttpContext context)
        {
            return Regex.IsMatch(context.Request.RawUrl, "^/stream/logs/?$", RegexOptions.IgnoreCase);
        }
    }
}
