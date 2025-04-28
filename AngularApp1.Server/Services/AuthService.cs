using AngularApp1.Server.Repositories.Interfaces;
using AngularApp1.Server.Services.Interfaces;
using AngularApp1.Server.Models.Entities;
using AngularApp1.Server.Models.Requests;
using AngularApp1.Server.Models.Responses;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AngularApp1.Server.Models;
using AngularApp1.Server.Exceptions;


namespace AngularApp1.Server.Services
{
    public sealed class AuthService : IAuthService
    {
        private const int TokenExpirationHours = 1;
        private const int RefreshTokenSize = 32;

        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthService> _logger;
        private readonly IConfiguration _configuration;

        public AuthService(
            IUserRepository userRepository,
            ITokenService tokenService,
            ILogger<AuthService> logger,
            IConfiguration configuration)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<LoginResponse> AuthenticateAsync(LoginRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var user = await _userRepository.GetByEmailAsync(request.Email)
                ?? throw new AuthenticationException("Invalid credentials");

            if (!VerifyPassword(request.Password, user.Password, user.Salt))
            {
                _logger.LogWarning("Invalid password for user: {Email}", request.Email);
                throw new AuthenticationException("Invalid credentials");
            }

            try
            {
                var token = await _tokenService.GenerateTokenAsync(user);
                var refreshToken = await GenerateRefreshTokenAsync();

                return new LoginResponse
                {
                    Token = token,
                    Expiration = DateTime.UtcNow.AddHours(TokenExpirationHours),
                    RefreshToken = refreshToken
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token generation failed for {Email}", request.Email);
                throw new AuthenticationException("Authentication failed");
            }
        }

        public async Task<AuthResult> RegisterAsync(RegisterRequest request)
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "User already exists with this email."
                };
            }

            var salt = GenerateSalt();
            var hashedPassword = HashPassword(request.Password, salt);

            var newUser = new User
            {
                Email = request.Email,
                Password = hashedPassword,
                Salt = salt,
                Role = request.Role ?? "user",
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.CreateAsync(newUser);

            return new AuthResult
            {
                Success = true,
                Message = "User registered successfully"
            };
        }

        private string GenerateSalt()
        {
            var saltBytes = new byte[16];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(saltBytes);
            return Convert.ToBase64String(saltBytes);
        }

        private string HashPassword(string password, string salt)
        {
            using var sha256 = SHA256.Create();
            var saltedPassword = $"{password}{salt}";
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
            return Convert.ToBase64String(hashBytes);
        }

        private bool VerifyPassword(string password, string storedHash, string salt)
        {
            using var sha256 = SHA256.Create();
            var saltedPassword = $"{password}{salt}";
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
            return Convert.ToBase64String(hashBytes) == storedHash;
        }

        public async Task<string> GenerateTokenAsync(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is missing in configuration"));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                Expires = DateTime.UtcNow.AddHours(TokenExpirationHours),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return await Task.FromResult(tokenHandler.WriteToken(token));
        }

        public async Task<string> GenerateRefreshTokenAsync()
        {
            var randomNumber = new byte[RefreshTokenSize];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return await Task.FromResult(Convert.ToBase64String(randomNumber));
        }

    }
}
