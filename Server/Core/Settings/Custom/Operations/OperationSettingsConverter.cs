using Batzill.Server.Core.Operations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Batzill.Server.Core.Settings.Custom.Operations
{
    public class OperationSettingsConverter : JsonConverter<OperationSettings>
    {
        private static Dictionary<string, Func<OperationSettings>> factoryMethods;
        private static OperationSettings CreateOperationSettings(string name)
        {
            if (factoryMethods == null)
            {
                factoryMethods = new Dictionary<string, Func<OperationSettings>>(StringComparer.InvariantCultureIgnoreCase)
                {
                    // Custom settings
                    { new DynamicOperation().Name, () => new DynamicOperationSettings() },
                    { new IdOperation().Name, () => new IdOperationSettings() },
                    { new AuthenticationOperation().Name, () => new AuthenticationOperationSettings() }
                };
            }

            if(factoryMethods.ContainsKey(name))
            {
                return factoryMethods[name]();
            }

            return new DefaultOperationSettings();
        }

        public override OperationSettings ReadJson(JsonReader reader, Type objectType, OperationSettings existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            // Load JObject from stream
            JObject jObject = JObject.Load(reader);

            // Get Name of operation
            string operationName = jObject[nameof(existingValue.Name)].ToString();

            // Initialize correct OperationSettings implemenation
            OperationSettings result = OperationSettingsConverter.CreateOperationSettings(operationName);

            serializer.Populate(jObject.CreateReader(), result);

            return result;
        }

        public override void WriteJson(JsonWriter writer, OperationSettings value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanWrite => false;
        public override bool CanRead => true;
    }
}
