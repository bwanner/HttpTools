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
            HashSet<string> logWriterRequests = new HashSet<string>(
                HttpServerRole.settingsProvider.Settings.Get(HttpServerSettingNames.LogWriter)
                .Split(
                    new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries
                    ));

            List<ILogWriter> logWriter = new List<ILogWriter>();

            if (logWriterRequests.Contains(HttpServerSettingValues.LogWriterConsole))
            {
                HttpServerRole.logger.Log(EventType.ServerSetup, "{0} logging enabled.", HttpServerSettingValues.LogWriterConsole);
                logWriter.Add(new ConsoleLogWriter());
            }
            if(logWriterRequests.Contains(HttpServerSettingValues.LogWriterFile))
            {
                HttpServerRole.logger.Log(EventType.ServerSetup, "{0} logging enabled.", HttpServerSettingValues.LogWriterFile);
                logWriter.Add(new FileLogWriter(new SystemFileWriter(), HttpServerRole.settingsProvider.Settings));
            }
            if(logWriterRequests.Contains(HttpServerSettingValues.LogWriterOperation))
            {
                HttpServerRole.logger.Log(EventType.ServerSetup, "{0} logging enabled.", HttpServerSettingValues.LogWriterOperation);
                logWriter.Add(new OperationLogWriter(new SystemFileWriter(), HttpServerRole.settingsProvider.Settings));
            }

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
            HttpServerRole.SetupLogging();
            HttpServerRole.operationFactory.ApplySettings(settings);
            HttpServerRole.httpServer.ApplySettings(settings);
        }
    }
}
