using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AngularApp1.Server.Services;
using AngularApp1.Server.Models.Responses;
using Google.Cloud.Dialogflow.V2;
using Google.Protobuf.Collections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AngularAppUnitTest.Services
{
    public class DialogflowServiceTests
    {
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IConfigurationSection> _dialogflowCredentialsSection;
        private readonly Mock<IConfigurationSection> _dialogflowProjectIdSection;
        private readonly Mock<ILogger<DialogflowService>> _loggerMock;
        private readonly Mock<SessionsClient> _sessionsClientMock;
        private readonly DialogflowService _service;

        public DialogflowServiceTests()
        {
            _configurationMock = new Mock<IConfiguration>();
            _dialogflowCredentialsSection = new Mock<IConfigurationSection>();
            _loggerMock = new Mock<ILogger<DialogflowService>>();
            _sessionsClientMock = new Mock<SessionsClient>();
            _dialogflowCredentialsSection.Setup(a => a.Value).Returns("YourCredentials");

            _configurationMock.Setup(c => c.GetSection("Dialogflow:Credentials"))
                .Returns(_dialogflowCredentialsSection.Object);
            _configurationMock.Setup(c => c["Dialogflow:ProjectId"])
                .Returns("YourProjectId");

            _service = new DialogflowService(
                _configurationMock.Object,
                _loggerMock.Object,
                _sessionsClientMock.Object);
        }

        [Fact]
        public async Task ProcessWebhookRequest_ShouldReturnResponse()
        {
            // Arrange
            var intent = new Intent { DisplayName = "TestIntent" };
            var queryResult = new QueryResult { Intent = intent };
            var request = new WebhookRequest { QueryResult = queryResult };

            // Act
            var response = await _service.ProcessWebhookRequest(request);

            // Assert
            Assert.Equal($"Processed intent: {intent.DisplayName}", response.FulfillmentText);
        }

        [Fact]
        public async Task ProcessWebhookRequest_ShouldHandleException()
        {
            // Arrange
            var queryResult = new QueryResult();
            var request = new WebhookRequest { QueryResult = queryResult };

            // Act
            var response = await _service.ProcessWebhookRequest(request);

            // Assert
            Assert.Equal("Sorry, an error occurred processing your request", response.FulfillmentText);
        }

        [Fact]
        public async Task DetectIntentWithDetailsAsync_ShouldReturn_TextGiven()
        {
            // Arrange
            var sessionId = "123";
            var text = "Hello, world!";
            var queryResult = new QueryResult
            {
                FulfillmentText = "Detected text",
                Intent = new Intent { DisplayName = "TestIntent" },
            };
            var responseMock = new DetectIntentResponse { QueryResult = queryResult };

            _sessionsClientMock.Setup(c => c.DetectIntentAsync(It.IsAny<DetectIntentRequest>(), null))
                .ReturnsAsync(responseMock);

            // Act
            var response = await _service.DetectIntentWithDetailsAsync(sessionId, text);

            // Assert
            Assert.Equal(queryResult.FulfillmentText, response.FulfillmentText);
        }

        [Fact]
        public async Task DetectIntentWithDetailsAsync_ShouldHandleDebugStandardBotExchangeCustomInput()
        {
            // Arrange
            var sessionId = "123";
            var text = "debugStandardBotExchangeCustomInput";

            // Mock response for custom payload
            var responseMock = new DetectIntentResponse { QueryResult = new QueryResult() };

            _sessionsClientMock.Setup(c => c.DetectIntentAsync(It.IsAny<DetectIntentRequest>(), null))
                .ReturnsAsync(responseMock);

            // Act
            var response = await _service.DetectIntentWithDetailsAsync(sessionId, text);

            // Assert
            Assert.Contains("Custom payload processed", response.Messages);
            Assert.Equal("Custom payload processed successfully", response.FulfillmentText);
        }


        [Fact]
        public async Task DetectIntentWithDetailsAsync_ShouldHandleException()
        {
            // Arrange
            var sessionId = "123";
            var text = "Hello";

            _sessionsClientMock.Setup(c => c.DetectIntentAsync(It.IsAny<DetectIntentRequest>(), null))
                .ThrowsAsync(new InvalidOperationException());

            // Act
            var response = await _service.DetectIntentWithDetailsAsync(sessionId, text);

            // Assert
            Assert.Equal("Sorry, I encountered an error processing your request.", response.FulfillmentText);
            Assert.Contains("Sorry, I encountered an error processing your request.", response.Messages);
        }
    }
}
