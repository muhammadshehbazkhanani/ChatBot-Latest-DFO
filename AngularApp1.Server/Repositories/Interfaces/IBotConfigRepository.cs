using AngularApp1.Server.Models.Entities;

namespace AngularApp1.Server.Repositories.Interfaces
{
    public interface IBotConfigRepository
    {
        Task<List<BotConfig>> GetAllAsync();
        Task<BotConfig> GetByIdAsync(string id);
        Task CreateAsync(BotConfig config);
        Task UpdateAsync(string id, BotConfig config);
        Task DeleteAsync(string id);
    }
}
