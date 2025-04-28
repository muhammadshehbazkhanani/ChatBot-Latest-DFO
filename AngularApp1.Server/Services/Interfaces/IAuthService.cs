using AngularApp1.Server.Models.Requests;
using AngularApp1.Server.Models.Responses;
using AngularApp1.Server.Models; 
using System.Threading.Tasks;

namespace AngularApp1.Server.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> AuthenticateAsync(LoginRequest request);
        Task<AuthResult> RegisterAsync(RegisterRequest request); 
    }
}
