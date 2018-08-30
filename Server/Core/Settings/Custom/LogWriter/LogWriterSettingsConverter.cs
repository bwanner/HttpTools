using Batzill.Server.Core.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Batzill.Server.Core.Settings.Custom.Operations
{
    public class LogWriterSettingsConverter : JsonConverter<LogWriterSettings>
    {
        private static Dictionary<string, Func<LogWriterSettings>> factoryMethods;
        private static LogWriterSettings CreateLogWriterSettings(string name)
        {
            if (factoryMethods == null)
            {
                factoryMethods = new Dictionary<string, Func<LogWriterSettings>>(StringComparer.InvariantCultureIgnoreCase)
                {
                    // Custom settings
                    { FileLogWriter.Name, () => new FileLogWriterSettings() },
                    { OperationLogWriter.Name, () => new OperationLogWriterSettings() }
                };
            }

            if(factoryMethods.ContainsKey(name))
            {
                return factoryMethods[name]();
            }

            return new DefaultLogWriterSettings();
        }

        public override LogWriterSettings ReadJson(JsonReader reader, Type objectType, LogWriterSettings existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            // Load JObject from stream
            JObject jObject = JObject.Load(reader);

            // Get Name of operation
            string logWriterName = jObject[nameof(existingValue.Name)].ToString();

            // Initialize correct OperationSettings implemenation
            LogWriterSettings result = LogWriterSettingsConverter.CreateLogWriterSettings(logWriterName);

            serializer.Populate(jObject.CreateReader(), result);

            return result;
        }

        public override void WriteJson(JsonWriter writer, LogWriterSettings value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanWrite => false;
        public override bool CanRead => true;
    }
}
