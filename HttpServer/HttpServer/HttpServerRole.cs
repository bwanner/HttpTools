using HttpServer.Core;
using HttpServer.Core.IO;
using HttpServer.Core.Logging;
using HttpServer.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer
{
    class HttpServerRole
    {
        private const string DefaultSettingsFilePath = "httpserver.config";

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

        private static void Initialize(string[] args)
        {
            // get args first
            string settingsFile = args == null || args.Length == 0 ? HttpServerRole.DefaultSettingsFilePath : args[0];

            HttpServerRole.settingsProvider = new HttpServerSettingsProvider(logger, new SytemFileReader(), settingsFile);

            HttpServerCore httpServer = new HttpClientServer(HttpServerRole.logger);
            httpServer.ApplySettings(HttpServerRole.settingsProvider.Settings);
        }
    }
}
