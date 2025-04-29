using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using AngularApp1.Server.Controllers;
using AngularApp1.Server.Models.Requests;
using AngularApp1.Server.Models.Responses;
using AngularApp1.Server.Services.Interfaces;
using AngularApp1.Server.Exceptions;
using AngularApp1.Server.Models;

namespace AngularAppUnitTest.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly Mock<ILogger<AuthController>> _mockLogger;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _mockLogger = new Mock<ILogger<AuthController>>();
            _controller = new AuthController(_mockAuthService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Register_ReturnsOk_WhenRegistrationSuccessful()
        {
            // Arrange
            var request = new RegisterRequest();
            _mockAuthService.Setup(x => x.RegisterAsync(request))
                .ReturnsAsync(new AuthResult { Success = true, Message = "Registered" });

            // Act
            var result = await _controller.Register(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value.GetType().GetProperty("result")?.GetValue(okResult.Value, null));
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenRegistrationFails()
        {
            // Arrange
            var request = new RegisterRequest();
            _mockAuthService.Setup(x => x.RegisterAsync(request))
                .ReturnsAsync(new AuthResult { Success = false, Message = "Email exists" });

            // Act
            var result = await _controller.Register(request);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.False((bool)badRequest.Value.GetType().GetProperty("result")?.GetValue(badRequest.Value, null));
        }

        [Fact]
        public async Task Login_ReturnsOk_WhenCredentialsValid()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "user@example.com", 
                Password = "password123"    
            };
            var loginResponse = new LoginResponse
            {
                Success = true, 
                Message = "Login success",
                Token = "fake-jwt-token",
                RefreshToken = "fake-refresh-token",
                Expiration = DateTime.UtcNow.AddHours(1)
            };
            _mockAuthService.Setup(x => x.AuthenticateAsync(request))
                .ReturnsAsync(loginResponse);

            // Act
            var result = await _controller.Login(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<LoginResponse>(okResult.Value);
            Assert.True(value.Success); 
        }

        [Fact]
        public async Task Login_ReturnsBadRequest_WhenAuthExceptionThrown()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "user@example.com", 
                Password = "password123"    
            };
            _mockAuthService.Setup(x => x.AuthenticateAsync(request))
                .ThrowsAsync(new AuthenticationException("Custom auth error"));

            // Act
            var result = await _controller.Login(request);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Custom auth error", badRequest.Value.ToString());
        }

        [Fact]
        public async Task Login_ReturnsBadRequest_WhenInvalidCredentials()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "user@example.com", 
                Password = "password123"    
            };
            _mockAuthService.Setup(x => x.AuthenticateAsync(request))
                .ThrowsAsync(new System.Exception("Invalid credentials"));

            // Act
            var result = await _controller.Login(request);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Invalid credentials", badRequest.Value.ToString());
        }

        [Fact]
        public async Task Login_Returns500_WhenUnhandledException()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "user@example.com",  
                Password = "password123"    
            };
            _mockAuthService.Setup(x => x.AuthenticateAsync(request))
                .ThrowsAsync(new System.Exception("Unexpected error"));

            // Act
            var result = await _controller.Login(request);

            // Assert
            var serverError = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, serverError.StatusCode);
        }

        [Fact]
        public void Options_ReturnsOk()
        {
            var result = _controller.Options();
            Assert.IsType<OkResult>(result);
        }
    }
}
