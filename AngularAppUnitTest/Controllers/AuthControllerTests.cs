using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using AngularApp1.Server.Controllers;
using AngularApp1.Server.Models.Requests;
using AngularApp1.Server.Models.Responses;
using AngularApp1.Server.Services.Interfaces;
using AngularApp1.Server.Exceptions;
using AngularApp1.Server.Models;
using Newtonsoft.Json.Linq;


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
            var request = new RegisterRequest();
            _mockAuthService.Setup(x => x.RegisterAsync(request))
                .ReturnsAsync(new AuthResult { Success = true, Message = "Registered" });

            var result = await _controller.Register(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = JObject.FromObject(okResult.Value!);
            Assert.True(json["result"]!.Value<bool>());
            Assert.Equal("Registered", json["message"]!.Value<string>());
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenRegistrationFails()
        {
            var request = new RegisterRequest();
            _mockAuthService.Setup(x => x.RegisterAsync(request))
                .ReturnsAsync(new AuthResult { Success = false, Message = "Email already exists" });

            var result = await _controller.Register(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var json = JObject.FromObject(badRequest.Value!);
            Assert.False(json["result"]!.Value<bool>());
            Assert.Equal("Email already exists", json["message"]!.Value<string>());
        }


        [Fact]
        public async Task Login_ReturnsOk_WhenCredentialsValid()
        {
            var request = new LoginRequest { Email = "test@test.com", Password = "pass" };
            var response = new LoginResponse
            {
                Success = true,
                Message = "Login success",
                Token = "token",
                RefreshToken = "refresh",
                Expiration = DateTime.UtcNow.AddMinutes(30)
            };

            _mockAuthService.Setup(x => x.AuthenticateAsync(request)).ReturnsAsync(response);

            var result = await _controller.Login(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var loginResponse = Assert.IsType<LoginResponse>(okResult.Value);
            Assert.True(loginResponse.Success);
            Assert.Equal("Login success", loginResponse.Message);
        }

        [Fact]
        public async Task Login_ReturnsBadRequest_WhenAuthenticationExceptionThrown()
        {
            var request = new LoginRequest { Email = "test@test.com", Password = "pass" };
            _mockAuthService.Setup(x => x.AuthenticateAsync(request))
                .ThrowsAsync(new AuthenticationException("Custom auth error"));

            var result = await _controller.Login(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var json = JObject.FromObject(badRequest.Value!);
            Assert.False(json["result"]!.Value<bool>());
            Assert.Equal("Custom auth error", json["message"]!.Value<string>());
        }


        [Fact]
        public async Task Login_ReturnsBadRequest_WhenInvalidCredentialsException()
        {
            var request = new LoginRequest { Email = "test@test.com", Password = "pass" };
            _mockAuthService.Setup(x => x.AuthenticateAsync(request))
                .ThrowsAsync(new Exception("Invalid credentials"));

            var result = await _controller.Login(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var json = JObject.FromObject(badRequest.Value!);
            Assert.False(json["result"]!.Value<bool>());
            Assert.Equal("Invalid credentials", json["message"]!.Value<string>());
        }


        [Fact]
        public async Task Login_Returns500_WhenUnhandledException()
        {
            var request = new LoginRequest { Email = "test@test.com", Password = "pass" };
            _mockAuthService.Setup(x => x.AuthenticateAsync(request))
                .ThrowsAsync(new Exception("Something else"));

            var result = await _controller.Login(request);

            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, errorResult.StatusCode);
            var json = JObject.FromObject(errorResult.Value!);
            Assert.False(json["result"]!.Value<bool>());
            Assert.Equal("An unexpected error occurred.", json["message"]!.Value<string>());
        }


        [Fact]
        public void Options_ReturnsOk()
        {
            var result = _controller.Options();
            Assert.IsType<OkResult>(result);
        }
    }
}
