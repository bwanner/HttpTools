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
using Batzill.Server.Core.Authentication;
using System.Linq;
using System.IO;
using Batzill.Server.Core.Exceptions;

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
        
        private static ConcurrentDictionary<string, DynamicResponse> Responses;

        public override string Name => "Dynamic";

        public DynamicOperation(Logger logger = null) : base(logger)
        {
        }

        protected override void InitializeClassInternal(OperationSettings settings, IAuthenticationManager authManager)
        {
            if (!(settings is DynamicOperationSettings))
            {
                throw new ArgumentException($"Type '{settings.GetType()}' is invalid for this operation.");
            }

            DynamicOperationSettings customSettings = settings as DynamicOperationSettings;

            DynamicOperation.Responses = new ConcurrentDictionary<string, DynamicResponse>();

            if (customSettings.Responses != null)
            {
                foreach (var responseEntry in customSettings.Responses)
                {
                    this.logger?.Log(EventType.OperationInformation, "Create DynamicResponse for id '{0}' using '{1}'.", responseEntry.Id, JsonConvert.SerializeObject(responseEntry.Response, Formatting.Indented));
                    DynamicOperation.Responses[responseEntry.Id] = responseEntry.Response;
                }
            }
        }

        protected override void ExecuteInternal(HttpContext context, IAuthenticationManager authManager)
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

                this.SetupResponse(context, id);

                return;
            }

            // it's a clear operation
            result = Regex.Match(context.Request.RawUrl, DynamicOperation.InputRegexClear, RegexOptions.IgnoreCase);
            id = result.Groups[1].Value;

            this.logger?.Log(EventType.OperationInformation, "Dynamic.Clear operation got called for id: {0}.", id);

            this.ClearResponse(context, id);
        }

        private DynamicResponse CreateDynamicResponseFromQueryString(string queryString)
        {
            DynamicResponse response = new DynamicResponse()
            {
                StatusCode = 0,
                Headers = new List<DynamicResponse.Header>(),
                Body = null
            };

            var parameters = HttpUtility.ParseQueryString(queryString);

            /* Parse StatusCode */
            if (string.IsNullOrEmpty(parameters[DynamicOperation.InputParameterStatusCode]))
            {
                this.logger?.Log(EventType.OperationError, "No status code provided.");
                throw new BadRequestException("No status code provided!");
            }

            if (!Int32.TryParse(parameters[DynamicOperation.InputParameterStatusCode], out int statusCode))
            {
                this.logger?.Log(EventType.OperationError, "Invalid status code privided: {0}.", parameters[DynamicOperation.InputParameterStatusCode]);
                throw new BadRequestException("Invalid status code provided!");
            }

            response.StatusCode = statusCode;

            this.logger?.Log(EventType.OperationInformation, "StatusCode: '{0}'.", response.StatusCode);

            /* Parse Header */
            if (!string.IsNullOrEmpty(parameters[DynamicOperation.InputParameterHeader]))
            {
                foreach (string header in parameters.GetValues(DynamicOperation.InputParameterHeader))
                {
                    Match res = Regex.Match(header, "^(.*):(.*)$");

                    if (!res.Success)
                    {
                        this.logger?.Log(EventType.OperationError, "Unable to match the regex with the provided header: {0}.", header);

                        throw new BadRequestException("Invalid header provided: '{0}'.", header);
                    }

                    string name = res.Groups[1].Value;
                    string value = res.Groups[2].Value;

                    response.Headers.Add(new DynamicResponse.Header()
                    {
                        Name = name,
                        Value = value
                    });

                    this.logger?.Log(EventType.OperationInformation, "Header: '{0}: {1}'.", name, value);
                }
            }

            /* Parse Body */
            response.Body = parameters[DynamicOperation.InputParameterBody];

            this.logger?.Log(EventType.OperationInformation, "Body: '{0}'.", this.TrimString(response.Body));

            return response;
        }

        private void WriteResponse(HttpContext context, string id)
        {
            DynamicResponse response = null;
            if (!DynamicOperation.Responses.ContainsKey(id) || (response = DynamicOperation.Responses[id]) == null)
            {
                this.logger?.Log(EventType.OperationInformation, "Id '{0}' isn't set up yet!", id);
                
                throw new NotFoundException($"Unable to find response for given id: '{id}'.");
            }

            this.logger?.Log(EventType.OperationInformation, "Found reponse for id '{0}': '{1}'.", id, this.TrimString(JsonConvert.SerializeObject(response, Formatting.Indented)));

            /* Set status code */
            context.Response.StatusCode = response.StatusCode;

            /* Set headers */
            if (response.Headers != null)
            {
                foreach (DynamicResponse.Header header in response.Headers)
                {
                    context.Response.Headers.Add(header.Name, header.Value);
                }
            }

            /* Set body */
            if (!string.IsNullOrEmpty(response.Body))
            {
                context.Response.WriteContent(response.Body);
            }
        }

        private void SetupResponse(HttpContext context, string id)
        {
            DynamicResponse response = null;

            // check if data is in body ...
            if (context.Request.HasEntityBody)
            {
                using (StreamReader reader = new StreamReader(context.Request.Stream))
                {
                    string bodyAsJson = null;
                    try
                    {
                        bodyAsJson = reader.ReadToEnd();
                    }
                    catch(Exception ex)
                    {
                        this.logger?.Log(EventType.OperationError, "Received the following exception when trying to read the request body: '{0}'", ex);

                        throw new BadRequestException("Failed to read the request body.");
                    }

                    if(string.IsNullOrEmpty(bodyAsJson))
                    {
                        this.logger?.Log(EventType.OperationError, "Empty request body was passed.");

                        throw new BadRequestException("Request body is empty.");
                    }

                    this.logger?.Log(EventType.OperationError, "Received the following request body: '{0}'.", this.TrimString(bodyAsJson));

                    try
                    {
                        response = JsonConvert.DeserializeObject<DynamicResponse>(bodyAsJson);
                    }
                    catch (Exception ex)
                    {
                        this.logger?.Log(EventType.OperationError, "Received the following exception when trying to deserialize the request body: '{0}'", ex);

                        throw new BadRequestException("Failed to deserialize the request body.");
                    }
                }
            }
            else
            {
                string responseLine = HttpUtility.UrlDecode(context.Request.Url.Query);
                response = this.CreateDynamicResponseFromQueryString(responseLine);
            }

            try
            {
                response.Validate();
            }
            catch(Exception ex)
            {
                if (ex is NullReferenceException || ex is IndexOutOfRangeException)
                {
                    this.logger?.Log(EventType.OperationError, "Received the following exception when validating the Response: '{0}'", ex);

                    throw new BadRequestException(ex.Message);
                }

                throw ex;
            }

            DynamicOperation.Responses[id] = response;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Successfully created response for id: '{id}':");
            sb.AppendLine($"StatusCode: {response.StatusCode}");

            sb.AppendLine("Headers:");
            if (response.Headers != null)
            {
                foreach (var header in response.Headers)
                {
                    sb.AppendLine($"    {header.Name}: {header.Value}");
                }
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

                throw new BadRequestException($"Unable to find response for given id: '{id}'.");
            }
            
            if (!DynamicOperation.Responses.TryRemove(id, out _))
            {
                this.logger?.Log(EventType.OperationInformation, "Error while trying to clear response for id '{0}'!", id);

                throw new InternalServerErrorException($"Unable to clear response for id: '{id}'. Please try again later.");
            }

            this.logger?.Log(EventType.OperationInformation, "Successfully cleared response for id '{0}'!", id);

            context.Response.StatusCode = 200;
            context.Response.WriteContent($"Successfully cleared response for id: '{id}'.");
        }

        private string TrimString(string value, int maxLength = 200)
        {
            if(string.IsNullOrEmpty(value) || value.Length < maxLength)
            {
                return value;
            }

            return string.Format("{0}...", value.Substring(0, maxLength - 3));
        }

        public override bool Match(HttpContext context)
        {
            return Regex.IsMatch(context.Request.RawUrl, DynamicOperation.InputRegexGet, RegexOptions.IgnoreCase)
                   || Regex.IsMatch(context.Request.RawUrl, DynamicOperation.InputRegexSet, RegexOptions.IgnoreCase)
                   || Regex.IsMatch(context.Request.RawUrl, DynamicOperation.InputRegexClear, RegexOptions.IgnoreCase);
        }

        public class DynamicResponse
        {
            [JsonProperty(Required = Required.Always)]
            public int StatusCode
            {
                get; set;
            }

            public List<Header> Headers
            {
                get; set;
            }

            public string Body
            {
                get; set;
            }

            public void Validate()
            {
                if (this.StatusCode < 100 || this.StatusCode > 999)
                {
                    throw new IndexOutOfRangeException($"'{nameof(this.StatusCode)}' must be within [100, 999]!");
                }
                
                if (this.Headers != null)
                {
                    this.Headers.ForEach((h) => h.Validate());
                }
            }

            public class Header
            {
                public string Name
                {
                    get; set;
                }

                public string Value
                {
                    get; set;
                }

                public void Validate()
                {
                    if (string.IsNullOrEmpty(this.Name))
                    {
                        throw new NullReferenceException($"'{nameof(this.Name)}' can't be null or empty!");
                    }

                    if (string.IsNullOrEmpty(this.Value))
                    {
                        throw new NullReferenceException($"'{nameof(this.Value)}' can't be null or empty!");
                    }
                }
            }
        }
    }
}
