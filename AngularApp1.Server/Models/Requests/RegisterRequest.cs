namespace AngularApp1.Server.Models.Requests
{
    public class RegisterRequest
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string Role { get; set; } = "user";
    }
}
