using Google.Cloud.Dialogflow.V2;
using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using AngularApp1.Server.Models.Responses;
using AngularApp1.Server.Services.Interfaces;
using AngularApp1.Server.Enums;
using DialogflowContext = Google.Cloud.Dialogflow.V2.Context;

namespace AngularApp1.Server.Services
{
    public class DialogflowService : IDialogflowService
    {
        private readonly string _projectId;
        private readonly SessionsClient _sessionsClient;
        private readonly ILogger<DialogflowService> _logger;
        private const string DebugCommand = "debugStandardBotExchangeCustomInput";
        private const string BranchOverrideCommand = "debugStandardBotBranchOverride";
        private const string VoiceBotOverrideIntent = "StandardBotBranchOverride";

        public DialogflowService(
            IConfiguration configuration,
            ILogger<DialogflowService> logger,
            SessionsClient sessionsClient = null)
        {
            _projectId = configuration["Dialogflow:ProjectId"] ??
                         throw new InvalidOperationException("Dialogflow ProjectId is required");
            _logger = logger;

            if (sessionsClient != null)
            {
                _sessionsClient = sessionsClient;
            }
            else
            {
                try
                {
                    _sessionsClient = new SessionsClientBuilder
                    {
                        JsonCredentials = configuration["Dialogflow:Credentials"]
                    }.Build();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create Dialogflow client");
                    throw;
                }
            }
        }

        public async Task<WebhookResponse> ProcessWebhookRequest(WebhookRequest request)
        {
            try
            {
                _logger.LogInformation($"Processing intent: {request.QueryResult.Intent.DisplayName}");
                return new WebhookResponse
                {
                    FulfillmentText = $"Processed intent: {request.QueryResult.Intent.DisplayName}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook request");
                return new WebhookResponse
                {
                    FulfillmentText = "Sorry, an error occurred processing your request"
                };
            }
        }

        public async Task<string> DetectIntentAsync(string sessionId, string text)
        {
            var response = await DetectIntentWithDetailsAsync(sessionId, text);
            return response.FulfillmentText;
        }

        public async Task<DialogflowResponse> DetectIntentWithDetailsAsync(string sessionId, string text)
        {
            try
            {
                if (text.Trim().Equals(DebugCommand, StringComparison.OrdinalIgnoreCase))
                {
                    return await SendCustomPayloadToDialogflow(sessionId);
                }

                var request = new DetectIntentRequest
                {
                    SessionAsSessionName = SessionName.FromProjectSession(_projectId, sessionId),
                    QueryInput = new QueryInput
                    {
                        Text = new TextInput
                        {
                            Text = text,
                            LanguageCode = "en-US"
                        }
                    }
                };

                var response = await _sessionsClient.DetectIntentAsync(request);
                _logger.LogInformation($"Dialogflow Response: {response.QueryResult}");


                return ProcessQueryResult(response.QueryResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dialogflow error");
                return new DialogflowResponse
                {
                    FulfillmentText = "Sorry, I encountered an error processing your request.",
                    ResultBranch = "ReturnControlToScript",
                    Messages = new List<string> { "Sorry, I encountered an error processing your request." }
                };
            }
        }

        private async Task<DialogflowResponse> SendCustomPayloadToDialogflow(string sessionId)
        {
            string json = @"{ 
                ""context"": { 
                    ""id"": ""autoContext"", 
                    ""lifespan"": ""1"", 
                    ""parameters"": { 
                        ""echoValue"": ""Passing Json to Bot method2"" 
                    } 
                } 
            }";

            var jsonStruct = Struct.Parser.ParseJson(json);
            var contextFields = jsonStruct.Fields["context"].StructValue.Fields;

            var context = new DialogflowContext
            {
                Name = $"projects/{_projectId}/agent/sessions/{sessionId}/contexts/{contextFields["id"].StringValue}",
                LifespanCount = int.Parse(contextFields["lifespan"].StringValue),
                Parameters = contextFields["parameters"].StructValue
            };

            var request = new DetectIntentRequest
            {
                SessionAsSessionName = SessionName.FromProjectSession(_projectId, sessionId),
                QueryInput = new QueryInput
                {
                    Text = new TextInput
                    {
                        Text = DebugCommand,
                        LanguageCode = "en-US"
                    }
                },
                QueryParams = new QueryParameters { Contexts = { context } }
            };

            var response = await _sessionsClient.DetectIntentAsync(request);

            if (string.IsNullOrEmpty(response.QueryResult.FulfillmentText))
            {
                return new DialogflowResponse
                {
                    FulfillmentText = "Custom payload processed successfully",
                    IntentName = DebugCommand,
                    Messages = new List<string>
                    {
                        "Custom payload processed",
                        $"echoValue: Passing Json to Bot method1",
                        $"context.echoValue: Passing Json to Bot method2"
                    },
                    ResultBranch = "PromptAndCollectNextResponse"
                };
            }

            return ProcessQueryResult(response.QueryResult);
        }

        private DialogflowResponse ProcessQueryResult(QueryResult queryResult)
        {
            var messages = queryResult.FulfillmentMessages
                .Where(m => m.MessageCase == Intent.Types.Message.MessageOneofCase.Text)
                .SelectMany(m => m.Text.Text_)
                .Where(text => !string.IsNullOrEmpty(text))
                .ToList();
            string overrideBranch = null;
            foreach (var message in queryResult.FulfillmentMessages)
            {
                if (message.MessageCase == Intent.Types.Message.MessageOneofCase.Payload)
                {
                    var jsonPayload = JObject.Parse(message.Payload.ToString());
                    if (jsonPayload["contentType"]?.ToString() == "ExchangeResultOverride")

                    {

                        var branch = jsonPayload["content"]?["vahExchangeResultBranch"]?.ToString();

                        if (!string.IsNullOrEmpty(branch))

                        {

                            overrideBranch = ValidateBranch(branch);

                            messages.Add($"[Branch Override]: {overrideBranch}");

                        }

                    }


                    if (jsonPayload["scriptpayloads"] != null)
                    {
                        messages.Add($"ScriptPayload: {jsonPayload["scriptpayloads"]}");
                    }
                }
            }

            return new DialogflowResponse
            {
                FulfillmentText = queryResult.FulfillmentText,
                IntentName = queryResult.Intent.DisplayName,
                Messages = messages,
                ResultBranch = overrideBranch ?? DetermineResultBranch(queryResult)
            };
        }

        private string DetermineResultBranch(QueryResult queryResult)
        {
            var intentType = IntentMappings.GetIntentType(queryResult.Intent.DisplayName);
            return IntentMappings.BranchMap[intentType];
        }

        private string ValidateBranch(string branch)

        {

            return IntentMappings.BranchMap.Values.Contains(branch)

                ? branch

                : "No Branch Found";

        }

    }
}