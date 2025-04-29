using AngularApp1.Server.Models;
using AngularApp1.Server.Models.Requests;
using AngularApp1.Server.Models.Responses;
using AngularApp1.Server.Repositories.Interfaces;
using AngularApp1.Server.Services;
using AngularApp1.Server.Services.Interfaces;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using AngularApp1.Server.Models.Entities;
using AngularApp1.Server.Exceptions;

namespace AngularApp1.Server.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<ILogger<AuthService>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockTokenService = new Mock<ITokenService>();
            _mockLogger = new Mock<ILogger<AuthService>>();
            _mockConfiguration = new Mock<IConfiguration>();

            _authService = new AuthService(
                _mockUserRepository.Object,
                _mockTokenService.Object,
                _mockLogger.Object,
                _mockConfiguration.Object
            );
        }

        private string HashPasswordForTest(string password, string salt)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var combined = password + salt;
            var bytes = System.Text.Encoding.UTF8.GetBytes(combined);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        [Fact]
        public async Task AuthenticateAsync_ValidCredentials_ReturnsLoginResponse()
        {
            // Arrange
            var plainPassword = "password";
            var salt = "testSalt";
            var hashedPassword = HashPasswordForTest(plainPassword, salt);

            var request = new LoginRequest
            {
                Email = "test@example.com",
                Password = plainPassword
            };

            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Email = request.Email,
                Password = hashedPassword,
                Salt = salt,
                Role = "user"
            };

            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(request.Email))
                .ReturnsAsync(user);
            _mockTokenService.Setup(service => service.GenerateTokenAsync(user))
                .ReturnsAsync("validToken");

            // Act
            var result = await _authService.AuthenticateAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("validToken", result.Token);
        }

        [Fact]
        public async Task AuthenticateAsync_InvalidCredentials_ThrowsAuthenticationException()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "test@example.com",
                Password = "wrongPassword"
            };

            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(request.Email))
                .ReturnsAsync((User)null);  // Simulate user not found

            // Act & Assert
            await Assert.ThrowsAsync<AuthenticationException>(
                () => _authService.AuthenticateAsync(request));
        }

        [Fact]
        public async Task RegisterAsync_Success_ReturnsAuthResult()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "newuser@example.com",
                Password = "password123",
                Role = "user"
            };

            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(request.Email))
                .ReturnsAsync((User)null);  // No existing user
            _mockUserRepository.Setup(repo => repo.CreateAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask); // Simulate success

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("User registered successfully", result.Message);
        }

        [Fact]
        public async Task RegisterAsync_UserAlreadyExists_ReturnsAuthResult()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "existinguser@example.com",
                Password = "password123",
                Role = "user"
            };

            var existingUser = new User
            {
                Id = Guid.NewGuid().ToString(),
                Email = request.Email,
                Password = "hashedPassword",
                Salt = "salt",
                Role = "user",
                CreatedAt = DateTime.UtcNow
            };

            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(request.Email))
                .ReturnsAsync(existingUser);

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User already exists with this email.", result.Message);
        }
    }
}
