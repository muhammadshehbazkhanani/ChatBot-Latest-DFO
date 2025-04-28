using Google.Cloud.Dialogflow.V2;
using AngularApp1.Server.Models.Responses;

namespace AngularApp1.Server.Services.Interfaces
{
    public interface IDialogflowService
    {
        Task<WebhookResponse> ProcessWebhookRequest(WebhookRequest request);
        Task<string> DetectIntentAsync(string sessionId, string text);
        Task<DialogflowResponse> DetectIntentWithDetailsAsync(string sessionId, string text);
    }
}