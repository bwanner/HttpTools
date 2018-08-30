using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Batzill.Server.Core.Logging;
using Batzill.Server.Core.Settings;
using Batzill.Server.Core.Settings.Custom.Operations;
using System.Collections.Concurrent;
using System.Web;
using HttpContext = Batzill.Server.Core.ObjectModel.HttpContext;
using Newtonsoft.Json;

namespace Batzill.Server.Core.Operations
{

    public class DynamicOperation : Operation
    {
        public const string InputRegexGet = @"^/dynamic/([a-zA-Z0-9\-_]+)/get$";
        public const string InputRegexSet = @"^/dynamic/([a-zA-Z0-9\-_]+)/set$";
        public const string InputRegexClear = @"^/dynamic/([a-zA-Z0-9\-_]+)/clear$";

        private const string InputParameterStatusCode = "statuscode";
        private const string InputParameterHeader = "header";
        private const string InputParameterBody = "body";

        public class DynamicResponse
        {
            [JsonProperty(Required = Required.Always)]
            public int StatusCode;
            public List<Tuple<string, string>> Headers;
            public string Body;
        }

        private static Logger Logger;
        private static ConcurrentDictionary<string, DynamicResponse> Responses;

        public override string Name => "Dynamic";

        public DynamicOperation() : base()
        {
        }

        public override void InitializeClass(OperationSettings settings)
        {
            if (!(settings is DynamicOperationSettings))
            {
                throw new ArgumentException($"Type '{settings.GetType()}' is invalid for this operation.");
            }

            DynamicOperationSettings customSettings = settings as DynamicOperationSettings;

            DynamicOperation.Logger = this.logger;
            DynamicOperation.Responses = new ConcurrentDictionary<string, DynamicResponse>();

            if (customSettings.Responses != null)
            {
                foreach (var responseEntry in customSettings.Responses)
                {
                    DynamicOperation.Logger?.Log(EventType.OperationInformation, "Create DynamicResponse for id '{0}' using '{1}'.", responseEntry.Id, JsonConvert.SerializeObject(responseEntry.Response, Formatting.Indented));
                    DynamicOperation.Responses[responseEntry.Id] = responseEntry.Response;
                }
            }
        }

        public override void Execute(HttpContext context)
        {
            string id;

            context.Response.SetDefaultValues();

            Match result = Regex.Match(context.Request.RawUrl, DynamicOperation.InputRegexGet, RegexOptions.IgnoreCase);
            if (result.Success)
            {
                id = result.Groups[1].Value;
                this.logger?.Log(EventType.OperationInformation, "Dynamic.Get operation got called for id: '{0}'.", id);

                this.WriteResponse(context, id);

                return;
            }

            result = Regex.Match(context.Request.RawUrl, DynamicOperation.InputRegexSet, RegexOptions.IgnoreCase);
            if (result.Success)
            {
                id = result.Groups[1].Value;
                this.logger?.Log(EventType.OperationInformation, "Dynamic.Set operation got called for id: {0}.", id);

                this.SetupResponse(context, id, HttpUtility.UrlDecode(context.Request.Url.Query));

                return;
            }

            // it's a clear operation
            result = Regex.Match(context.Request.RawUrl, DynamicOperation.InputRegexClear, RegexOptions.IgnoreCase);
            id = result.Groups[1].Value;

            this.logger?.Log(EventType.OperationInformation, "Dynamic.Clear operation got called for id: {0}.", id);

            this.ClearResponse(context, id);
        }

        private static DynamicResponse CreateResponse(string responseLine)
        {
            DynamicResponse response = new DynamicResponse()
            {
                StatusCode = 0,
                Headers = new List<Tuple<string, string>>(),
                Body = null
            };

            var parameters = HttpUtility.ParseQueryString(responseLine);

            /* Parse StatusCode */
            if (string.IsNullOrEmpty(parameters[DynamicOperation.InputParameterStatusCode]))
            {
                DynamicOperation.Logger?.Log(EventType.OperationError, "No status code provided.");
                throw new InvalidOperationException("No status code provided!");
            }

            if (!Int32.TryParse(parameters[DynamicOperation.InputParameterStatusCode], out response.StatusCode) || response.StatusCode < 100 || response.StatusCode > 1000)
            {
                DynamicOperation.Logger?.Log(EventType.OperationError, "Invalid status code privided: {0}.", parameters[DynamicOperation.InputParameterStatusCode]);
                throw new InvalidOperationException("Invalid status code provided!");
            }

            DynamicOperation.Logger?.Log(EventType.OperationInformation, "StatusCode: '{0}'.", response.StatusCode);

            /* Parse Header */
            if (!string.IsNullOrEmpty(parameters[DynamicOperation.InputParameterHeader]))
            {
                foreach (string header in parameters.GetValues(DynamicOperation.InputParameterHeader))
                {
                    Match res = Regex.Match(header, "^(.*):(.*)$");

                    if (!res.Success)
                    {
                        DynamicOperation.Logger?.Log(EventType.OperationError, "Invalid header privided: {0}.", header);
                        throw new InvalidOperationException("Invalid header provided!");
                    }

                    string name = res.Groups[1].Value;
                    string value = res.Groups[2].Value;

                    if (string.IsNullOrEmpty(name))
                    {
                        DynamicOperation.Logger?.Log(EventType.OperationError, "Invalid header privided. Header name can't be empty.", header);
                        throw new InvalidOperationException("Invalid header provided!");
                    }

                    response.Headers.Add(new Tuple<string, string>(name, value));

                    DynamicOperation.Logger?.Log(EventType.OperationInformation, "Header: '{0}: {1}'.", name, value);
                }
            }

            /* Parse Body */
            response.Body = parameters[DynamicOperation.InputParameterBody];

            DynamicOperation.Logger?.Log(EventType.OperationInformation, "Body: '{0}'.", response.Body);

            return response;
        }

        private void WriteResponse(HttpContext context, string id)
        {
            DynamicResponse response = null;
            if (!DynamicOperation.Responses.ContainsKey(id) || (response = DynamicOperation.Responses[id]) == null)
            {
                this.logger?.Log(EventType.OperationInformation, "Id '{0}' isn't set up yet!", id);

                context.Response.StatusCode = 404;
                context.Response.WriteContent($"Unable to find response for given id: '{id}'.");

                return;
            }
            
            /* Set status code */
            context.Response.StatusCode = response.StatusCode;

            /* Set headers */
            foreach (Tuple<string, string> header in response.Headers)
            {
                context.Response.Headers.Add(header.Item1, header.Item2);
            }

            /* Set body */
            if (!string.IsNullOrEmpty(response.Body))
            {
                context.Response.WriteContent(response.Body);
            }
        }

        private void SetupResponse(HttpContext context, string id, string responseLine)
        {
            DynamicResponse response = DynamicOperation.CreateResponse(responseLine);
            DynamicOperation.Responses[id] = response;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Successfully created response for id: '{id}':");
            sb.AppendLine($"StatusCode: {response.StatusCode}");
            sb.AppendLine("Headers:");
            foreach (var header in response.Headers)
            {
                sb.AppendLine($"    {header.Item1}: {header.Item2}");
            }
            sb.AppendLine("Body:");
            sb.AppendLine($"    {response.Body}");

            context.Response.StatusCode = 200;
            context.Response.WriteContent(sb.ToString());
        }

        private void ClearResponse(HttpContext context, string id)
        {
            if (!DynamicOperation.Responses.ContainsKey(id))
            {
                this.logger?.Log(EventType.OperationInformation, "Id '{0}' isn't set up yet!", id);

                context.Response.StatusCode = 404;
                context.Response.WriteContent($"Unable to find response for given id: '{id}'.");

                return;
            }
            
            if (!DynamicOperation.Responses.TryRemove(id, out _))
            {
                this.logger?.Log(EventType.OperationInformation, "Error while trying to clear response for id '{0}'!", id);

                context.Response.StatusCode = 500;
                context.Response.WriteContent($"Error while trying to clear response for id: '{id}'. Please try again later.");

                return;
            }

            this.logger?.Log(EventType.OperationInformation, "Successfully cleared response for id '{0}'!", id);

            context.Response.StatusCode = 200;
            context.Response.WriteContent($"Successfully cleared response for id: '{id}'.");
        }

        public override bool Match(HttpContext context)
        {
            return Regex.IsMatch(context.Request.RawUrl, DynamicOperation.InputRegexGet, RegexOptions.IgnoreCase)
                   || Regex.IsMatch(context.Request.RawUrl, DynamicOperation.InputRegexSet, RegexOptions.IgnoreCase)
                   || Regex.IsMatch(context.Request.RawUrl, DynamicOperation.InputRegexClear, RegexOptions.IgnoreCase);
        }
    }
}
