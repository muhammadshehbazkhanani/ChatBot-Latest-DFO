using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using AngularApp1.Server.Controllers;
using AngularApp1.Server.Models.Entities;
using AngularApp1.Server.Models.Requests;
using AngularApp1.Server.Services.Interfaces;


namespace AngularAppUnitTest.Controllers

{
    public class BotConfigsControllerTests
    {
        private readonly Mock<IBotConfigService> _mockBotConfigService;
        private readonly Mock<ILogger<BotConfigsController>> _mockLogger;
        private readonly BotConfigsController _controller;

        public BotConfigsControllerTests()
        {
            _mockBotConfigService = new Mock<IBotConfigService>();
            _mockLogger = new Mock<ILogger<BotConfigsController>>();
            _controller = new BotConfigsController(_mockBotConfigService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOk_WithConfigs()
        {
            var configs = new List<BotConfig> { new BotConfig { Id = "1", AppName = "App" } };
            _mockBotConfigService.Setup(s => s.GetAsync()).ReturnsAsync(configs);

            var result = await _controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<List<AngularApp1.Server.Models.Responses.BotConfigResponse>>(okResult.Value);
        }

        [Fact]
        public async Task GetAll_Returns500_OnException()
        {
            _mockBotConfigService.Setup(s => s.GetAsync()).ThrowsAsync(new Exception("err"));

            var result = await _controller.GetAll();

            var objResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objResult.StatusCode);
            Assert.Equal("Error retrieving configurations", objResult.Value);
        }

        [Fact]
        public async Task GetById_ReturnsOk_WhenFound()
        {
            var config = new BotConfig { Id = "1", AppName = "App" };
            _mockBotConfigService.Setup(s => s.GetAsync("1")).ReturnsAsync(config);

            var result = await _controller.GetById("1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<AngularApp1.Server.Models.Responses.BotConfigResponse>(okResult.Value);
        }

        [Fact]
        public async Task GetById_Returns404_WhenNotFound()
        {
            _mockBotConfigService.Setup(s => s.GetAsync("1")).ReturnsAsync((BotConfig)null!);

            var result = await _controller.GetById("1");

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetById_Returns500_OnException()
        {
            _mockBotConfigService.Setup(s => s.GetAsync("1")).ThrowsAsync(new Exception("error"));

            var result = await _controller.GetById("1");

            var objResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objResult.StatusCode);
            Assert.Equal("Error retrieving configuration", objResult.Value);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtAction()
        {
            var request = new CreateBotConfigRequest
            {
                AppName = "App",
                Config1 = "C1",
                Config2 = "C2",
                Config3 = "C3"
            };

            _mockBotConfigService.Setup(s => s.CreateAsync(It.IsAny<BotConfig>()))
                .Callback<BotConfig>(config => config.Id = "123");

            var result = await _controller.Create(request);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal("GetById", createdResult.ActionName);
            Assert.IsType<AngularApp1.Server.Models.Responses.BotConfigResponse>(createdResult.Value);
        }

        [Fact]
        public async Task Create_Returns400_WhenModelInvalid()
        {
            _controller.ModelState.AddModelError("AppName", "Required");

            var result = await _controller.Create(new CreateBotConfigRequest());

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badRequest.Value);
        }

        [Fact]
        public async Task Create_Returns500_OnException()
        {
            var request = new CreateBotConfigRequest { AppName = "App" };

            _mockBotConfigService.Setup(s => s.CreateAsync(It.IsAny<BotConfig>()))
                .ThrowsAsync(new Exception("error"));

            var result = await _controller.Create(request);

            var objResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objResult.StatusCode);
            Assert.Equal("Error creating configuration", objResult.Value);
        }

        [Fact]
        public async Task Update_ReturnsOk_WhenSuccessful()
        {
            var request = new CreateBotConfigRequest { AppName = "App" };
            var existing = new BotConfig { Id = "1", AppName = "Old" };

            _mockBotConfigService.Setup(s => s.GetAsync("1")).ReturnsAsync(existing);

            var result = await _controller.Update("1", request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(existing, okResult.Value);
        }

        [Fact]
        public async Task Update_Returns400_WhenModelInvalid()
        {
            _controller.ModelState.AddModelError("AppName", "Required");

            var result = await _controller.Update("1", new CreateBotConfigRequest());

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badRequest.Value);
        }

        [Fact]
        public async Task Update_Returns404_WhenNotFound()
        {
            _mockBotConfigService.Setup(s => s.GetAsync("1")).ReturnsAsync((BotConfig)null!);

            var result = await _controller.Update("1", new CreateBotConfigRequest());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Update_Returns500_OnException()
        {
            var config = new BotConfig { Id = "1" };
            _mockBotConfigService.Setup(s => s.GetAsync("1")).ReturnsAsync(config);
            _mockBotConfigService.Setup(s => s.UpdateAsync("1", config)).ThrowsAsync(new Exception("update"));

            var result = await _controller.Update("1", new CreateBotConfigRequest { AppName = "App" });

            var objResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objResult.StatusCode);
            Assert.Equal("Error updating configuration", objResult.Value);
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_WhenSuccessful()
        {
            var config = new BotConfig { Id = "1" };
            _mockBotConfigService.Setup(s => s.GetAsync("1")).ReturnsAsync(config);

            var result = await _controller.Delete("1");

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_Returns404_WhenNotFound()
        {
            _mockBotConfigService.Setup(s => s.GetAsync("1")).ReturnsAsync((BotConfig)null!);

            var result = await _controller.Delete("1");

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_Returns500_OnException()
        {
            var config = new BotConfig { Id = "1" };
            _mockBotConfigService.Setup(s => s.GetAsync("1")).ReturnsAsync(config);
            _mockBotConfigService.Setup(s => s.RemoveAsync("1")).ThrowsAsync(new Exception("delete"));

            var result = await _controller.Delete("1");

            var objResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objResult.StatusCode);
            Assert.Equal("Error deleting configuration", objResult.Value);
        }
    }

}