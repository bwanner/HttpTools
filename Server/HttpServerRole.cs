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
using Batzill.Server.Core.SSLBindingHelper;

namespace Batzill.Server
{
    class HttpServerRole
    {
        private const string DefaultSettingsFilePath = "default.cfg";

        private static Logger logger;
        private static HttpServer httpServer;
        private static IOperationFactory operationFactory;
        private static HttpServerSettingsProvider settingsProvider;

        static void Main(string[] args)
        {
            HttpServerRole.SetupBasicLogging();
            HttpServerRole.Initialize(args);
            HttpServerRole.Start();

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
            // get settings first
            string settingsFile = args == null || args.Length == 0 ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, HttpServerRole.DefaultSettingsFilePath) : args[0];

            HttpServerRole.settingsProvider = new HttpServerSettingsProvider(logger, new SystemFileReader(), settingsFile);

            // setup regular logging
            HttpServerRole.SetupLogging();

            // setup operation factory
            HttpServerRole.operationFactory = new AssemblyOperationFactory(HttpServerRole.logger, HttpServerRole.settingsProvider.Settings);

            // setup httpServer
            HttpServerRole.httpServer = new HttpClientServer(
                HttpServerRole.logger,
                HttpServerRole.operationFactory,
                new TaskFactory(), 
                HttpServerRole.settingsProvider.Settings,
                new NetshWrapper(HttpServerRole.logger));
        }

        private static void Start()
        {
            while (!HttpServerRole.httpServer.Start())
            {
                Console.Write("Start failed, press ENTER to try again.");
                Console.ReadLine();
            }
        }

        private static void SettingsChangedEventHandler(HttpServerSettings settings)
        {
            HttpServerRole.logger.ApplySettings(settings);
            HttpServerRole.operationFactory.ApplySettings(settings);
            HttpServerRole.httpServer.ApplySettings(settings);
        }
    }
}
