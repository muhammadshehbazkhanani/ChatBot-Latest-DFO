using AngularApp1.Server.Models.Requests;
using AngularApp1.Server.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AngularApp1.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);

            if (!result.Success)
            {
                return BadRequest(new { result = false, message = result.Message });
            }

            return Ok(new { result = true, message = result.Message });
        }

        [HttpOptions]
        public IActionResult Options()
        {
            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var result = await _authService.AuthenticateAsync(request);
                return Ok(result);
            }
            catch (AngularApp1.Server.Exceptions.AuthenticationException ex) 
            {
                return BadRequest(new { result = false, message = ex.Message });
            }
            catch (Exception ex) when (ex.Message == "Invalid credentials")
            {
                return BadRequest(new { result = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while logging in.");
                return StatusCode(500, new { result = false, message = "An unexpected error occurred." });
            }
        }

    }
}