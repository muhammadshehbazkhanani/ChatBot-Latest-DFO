using Moq;
using AngularApp1.Server.Models.Entities;
using AngularApp1.Server.Services;
using AngularApp1.Server.Repositories.Interfaces;

namespace AngularAppUnitTest.Services
{
    public class BotConfigServiceTests
    {
        private readonly Mock<IBotConfigRepository> _mockRepo;
        private readonly BotConfigService _service;

        public BotConfigServiceTests()
        {
            _mockRepo = new Mock<IBotConfigRepository>();
            _service = new BotConfigService(_mockRepo.Object);
        }

        [Fact]
        public async Task GetAsync_ShouldReturnBotConfigsList()
        {
            // Arrange
            var botConfigs = new List<BotConfig>
            {
                new BotConfig { Id = "1", AppName = "App1" },
                new BotConfig { Id = "2", AppName = "App2" }
            };
            _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(botConfigs);

            // Act
            var result = await _service.GetAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("App1", result[0].AppName);
        }

        [Fact]
        public async Task GetAsync_ById_ShouldReturnBotConfig()
        {
            // Arrange
            var botConfig = new BotConfig { Id = "123", AppName = "TestApp" };
            _mockRepo.Setup(r => r.GetByIdAsync("123")).ReturnsAsync(botConfig);

            // Act
            var result = await _service.GetAsync("123");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TestApp", result.AppName);
        }

        [Fact]
        public async Task CreateAsync_ShouldCallInsertOneAsync()
        {
            // Arrange
            var newConfig = new BotConfig { Id = "1", AppName = "App1" };

            // Act
            await _service.CreateAsync(newConfig);

            // Assert
            _mockRepo.Verify(r => r.CreateAsync(newConfig), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldCallReplaceOneAsync()
        {
            // Arrange
            var config = new BotConfig { Id = "1", AppName = "UpdatedApp" };

            // Act
            await _service.UpdateAsync("1", config);

            // Assert
            _mockRepo.Verify(r => r.UpdateAsync("1", config), Times.Once);
        }

        [Fact]
        public async Task RemoveAsync_ShouldCallDeleteOneAsync()
        {
            // Act
            await _service.RemoveAsync("1");

            // Assert
            _mockRepo.Verify(r => r.DeleteAsync("1"), Times.Once);
        }
    }
}
