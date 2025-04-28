using AngularApp1.Server.Services.Interfaces;
using Google.Cloud.Dialogflow.V2;
using Google.Protobuf;
using Microsoft.AspNetCore.Mvc;
using AngularApp1.Server.Models.Requests;

[ApiController]
[Route("api/dialogflow")]
public class DialogflowController : ControllerBase
{
    private readonly IDialogflowService _dialogflowService;

    public DialogflowController(IDialogflowService dialogflowService)
    {
        _dialogflowService = dialogflowService;
    }

    [HttpPost("detect-intent")]
    public async Task<IActionResult> DetectIntent([FromBody] DialogflowRequest request)
    {
        try
        {
            var response = await _dialogflowService.DetectIntentWithDetailsAsync(request.SessionId, request.Text);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> HandleWebhook()
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();

        try
        {
            var parser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));
            var webhookRequest = parser.Parse<WebhookRequest>(body);
            var webhookResponse = await _dialogflowService.ProcessWebhookRequest(webhookRequest);
            return Content(webhookResponse.ToString(), "application/json");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error processing webhook: {ex.Message}");
        }
    }
}
