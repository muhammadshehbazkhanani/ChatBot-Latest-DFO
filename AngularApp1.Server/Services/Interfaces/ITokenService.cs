using AngularApp1.Server.Models.Entities;
using System.Threading.Tasks;

namespace AngularApp1.Server.Services.Interfaces
{
    public interface ITokenService
    {
        Task<string> GenerateTokenAsync(User user);
        Task<string> GenerateRefreshTokenAsync();
    }
}