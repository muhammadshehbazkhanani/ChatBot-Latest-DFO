using Xunit;
using Microsoft.AspNetCore.Mvc;
using AngularApp1.Server.Controllers;
using AngularApp1.Server.Models.Requests;
using AngularApp1.Server.Models.Responses;
using AngularApp1.Server.Services.Interfaces;
using Moq;
using System.Threading.Tasks;
using System.Collections.Generic;
using AngularApp1.Server.Models.Entities;
using Microsoft.Extensions.Logging;

namespace AngularAppUnitTest.Controllers
{
    public class BotConfigControllerTests
    {
        private readonly Mock<IBotConfigService> _mockBotConfigService;
        private readonly BotConfigsController _controller;

        public BotConfigControllerTests()
        {
            _mockBotConfigService = new Mock<IBotConfigService>();
            _controller = new BotConfigsController(_mockBotConfigService.Object, Mock.Of<ILogger<BotConfigsController>>());
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithListOfBotConfigResponse()
        {
            // Arrange
            var botConfigs = new List<BotConfig>
            {
                new BotConfig { Id = "1", AppName = "Bot1", Config1 = "Config1", Config2 = "Config2", Config3 = "Config3" },
                new BotConfig { Id = "2", AppName = "Bot2", Config1 = "Config1", Config2 = "Config2", Config3 = "Config3" }
            };

            _mockBotConfigService.Setup(service => service.GetAsync()).ReturnsAsync(botConfigs);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = okResult?.Value as List<BotConfigResponse>;

            Assert.NotNull(returnValue);
            Assert.Equal(2, returnValue?.Count);
            Assert.Equal("Bot1", returnValue?[0].AppName);
            Assert.Equal("Bot2", returnValue?[1].AppName);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenConfigDoesNotExist()
        {
            // Arrange
            var nonExistentId = "non-existent-id";
            _mockBotConfigService.Setup(service => service.GetAsync(nonExistentId)).ReturnsAsync((BotConfig?)null);

            // Act
            var result = await _controller.GetById(nonExistentId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult_WithBotConfigResponse()
        {
            // Arrange
            var configId = "1";
            var botConfig = new BotConfig { Id = configId, AppName = "Bot1", Config1 = "Config1", Config2 = "Config2", Config3 = "Config3" };
            _mockBotConfigService.Setup(service => service.GetAsync(configId)).ReturnsAsync(botConfig);

            // Act
            var result = await _controller.GetById(configId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = okResult?.Value as BotConfigResponse;

            Assert.NotNull(returnValue);
            Assert.Equal(configId, returnValue?.Id);
            Assert.Equal("Bot1", returnValue?.AppName);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtAction_WithBotConfigResponse()
        {
            // Arrange
            var request = new CreateBotConfigRequest
            {
                AppName = "Bot1",
                Config1 = "Config1",
                Config2 = "Config2",
                Config3 = "Config3"
            };

            // Create a new BotConfig with a set ID (simulate the service behavior)
            var newBotConfig = new BotConfig
            {
                Id = "1",  // Ensure Id is set here (mocking behavior of DB assigning an Id)
                AppName = "Bot1",
                Config1 = "Config1",
                Config2 = "Config2",
                Config3 = "Config3"
            };

            // Setup the mock service to simulate that CreateAsync is being called and the ID is assigned
            _mockBotConfigService.Setup(service => service.CreateAsync(It.IsAny<BotConfig>()))
                                 .Callback<BotConfig>((botConfig) => botConfig.Id = "1") // Simulate ID assignment
                                 .Returns(Task.CompletedTask);

            // Setup the service to return the created BotConfig when GetAsync is called
            _mockBotConfigService.Setup(service => service.GetAsync("1")).ReturnsAsync(newBotConfig);

            // Act
            var result = await _controller.Create(request);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnValue = createdAtActionResult?.Value as BotConfigResponse;

            Assert.NotNull(returnValue); // Check that the response is not null
            Assert.Equal("1", returnValue?.Id); // Assert that the ID is correctly returned
            Assert.Equal("Bot1", returnValue?.AppName); // Assert other fields as well
        }


        [Fact]
        public async Task Update_ReturnsNotFound_WhenConfigDoesNotExist()
        {
            // Arrange
            var configId = "non-existent-id";
            var request = new CreateBotConfigRequest { AppName = "Bot1", Config1 = "Config1", Config2 = "Config2", Config3 = "Config3" };
            _mockBotConfigService.Setup(service => service.GetAsync(configId)).ReturnsAsync((BotConfig?)null);

            // Act
            var result = await _controller.Update(configId, request);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_WhenConfigIsDeleted()
        {
            // Arrange
            var configId = "1";
            var botConfig = new BotConfig { Id = configId, AppName = "Bot1", Config1 = "Config1", Config2 = "Config2", Config3 = "Config3" };
            _mockBotConfigService.Setup(service => service.GetAsync(configId)).ReturnsAsync(botConfig);
            _mockBotConfigService.Setup(service => service.RemoveAsync(configId)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(configId);

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);
        }
    }
}
