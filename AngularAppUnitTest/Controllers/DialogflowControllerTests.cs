using AngularApp1.Server.Controllers;
using AngularApp1.Server.Models.Responses;
using AngularApp1.Server.Models.Requests;
using AngularApp1.Server.Services.Interfaces;
using Google.Cloud.Dialogflow.V2;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AngularAppUnitTest.Controllers
{
    public class DialogflowControllerTests
    {
        private readonly Mock<IDialogflowService> _dialogflowServiceMock;
        private readonly DialogflowController _controller;

        public DialogflowControllerTests()
        {
            _dialogflowServiceMock = new Mock<IDialogflowService>();
            _controller = new DialogflowController(_dialogflowServiceMock.Object);
        }

        [Fact]
        public async Task DetectIntent_ReturnsOk_WithValidResponse()
        {
            // Arrange
            var request = new DialogflowRequest { SessionId = "test-session", Text = "hello" };
            var expectedResponse = new DialogflowResponse { FulfillmentText = "Hi!", IntentName = "Greeting" };

            _dialogflowServiceMock
                .Setup(s => s.DetectIntentWithDetailsAsync(request.SessionId, request.Text))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.DetectIntent(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            var response = Assert.IsType<DialogflowResponse>(okResult.Value);
            Assert.Equal("Hi!", response.FulfillmentText);
        }

        [Fact]
        public async Task DetectIntent_Returns500_OnException()
        {
            // Arrange
            var mockService = new Mock<IDialogflowService>();
            var controller = new DialogflowController(mockService.Object);
            var request = new DialogflowRequest
            {
                SessionId = "test-session",
                Text = "trigger error"
            };

            mockService
                .Setup(s => s.DetectIntentWithDetailsAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await controller.DetectIntent(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);

            // Using Newtonsoft.Json to safely handle dynamic object
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(objectResult.Value);
            var dynamicObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);
            Assert.Equal("Test exception", dynamicObject.error.ToString());
        }

        [Fact]
        public async Task HandleWebhook_ReturnsParsedResponse()
        {
            // Arrange
            var webhookRequest = new WebhookRequest
            {
                QueryResult = new QueryResult
                {
                    Intent = new Intent { DisplayName = "TestIntent" }
                }
            };

            var expectedResponse = new WebhookResponse { FulfillmentText = "Processed intent: TestIntent" };

            var json = webhookRequest.ToString(); // Protobuf format as JSON
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { Request = { Body = stream } }
            };

            _dialogflowServiceMock
                .Setup(s => s.ProcessWebhookRequest(It.IsAny<WebhookRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.HandleWebhook();

            // Assert
            var contentResult = Assert.IsType<ContentResult>(result);
            Assert.Equal("application/json", contentResult.ContentType);
            Assert.NotNull(contentResult.Content);
            Assert.Contains("Processed intent: TestIntent", contentResult.Content);
        }

        [Fact]
        public async Task HandleWebhook_ReturnsBadRequest_OnInvalidJson()
        {
            // Arrange
            var invalidJson = "{ this is invalid json }";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(invalidJson));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { Request = { Body = stream } }
            };

            // Act
            var result = await _controller.HandleWebhook();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.NotNull(badRequestResult.Value);
            Assert.Contains("Error processing webhook", badRequestResult.Value.ToString()!);
        }
    }
}