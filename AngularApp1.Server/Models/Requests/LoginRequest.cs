
namespace AngularApp1.Server.Models.Requests
{
    public class LoginRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}