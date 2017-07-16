using Batzill.Server.Core;
using Batzill.Server.Core.IO;
using Batzill.Server.Core.Logging;
using Batzill.Server.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Batzill.Server
{
    class HttpServerRole
    {
        private const string DefaultSettingsFilePath = "default.cfg";

        private static HttpServerSettingsProvider settingsProvider;
        private static Logger logger;

        static void Main(string[] args)
        {
            HttpServerRole.SetupBasicLogging();
            HttpServerRole.Initialize(args);


            Console.ReadLine();
        }

        private static void SetupBasicLogging()
        {
            HttpServerRole.logger = new BasicLogger(new ConsoleLogWriter());
        }

        private static void SetupLogging()
        {
            List<ILogWriter> logWriter = new List<ILogWriter>();
            logWriter.Add(new ConsoleLogWriter());
            logWriter.Add(new FrontendOperationLogWriter(new SystemFileWriter(), HttpServerRole.settingsProvider.Settings));
            logWriter.Add(new FileLogWriter(new SystemFileWriter(), HttpServerRole.settingsProvider.Settings));

            HttpServerRole.logger = new BasicLogger(new MultiLogWriter(logWriter));
        }

        private static void Initialize(string[] args)
        {
            // get args first
            string settingsFile = args == null || args.Length == 0 ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, HttpServerRole.DefaultSettingsFilePath) : args[0];

            // get setting!
            HttpServerRole.settingsProvider = new HttpServerSettingsProvider(logger, new SystemFileReader(), settingsFile);

            // setup regular logging
            HttpServerRole.SetupLogging();

            HttpServer httpServer = new HttpClientServer(HttpServerRole.logger, HttpServerRole.settingsProvider.Settings, new AssemblyOperationFactory(HttpServerRole.logger), new TaskFactory());

            httpServer.Start();
        }
    }
}
