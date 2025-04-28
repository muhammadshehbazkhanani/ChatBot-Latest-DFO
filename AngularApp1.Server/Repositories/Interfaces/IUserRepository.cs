using AngularApp1.Server.Models.Entities;

namespace AngularApp1.Server.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task CreateAsync(User user);
    }
}
