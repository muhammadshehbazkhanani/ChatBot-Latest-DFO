using AngularApp1.Server.Models.Entities;
using AngularApp1.Server.Repositories.Interfaces;
using MongoDB.Driver;

namespace AngularApp1.Server.Repositories
{
    public sealed class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _users;

        public UserRepository(IMongoDatabase database)
        {
            _users = database.GetCollection<User>("Users");
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _users.Find(user => user.Email == email).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(User user)
        {
            await _users.InsertOneAsync(user);
        }
    }
}
