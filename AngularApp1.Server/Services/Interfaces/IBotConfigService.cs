using AngularApp1.Server.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using AngularApp1.Server.Models.Entities;

namespace AngularApp1.Server.Services.Interfaces
{
    public interface IBotConfigService
    {
        Task<List<BotConfig>> GetAsync();
        Task<BotConfig?> GetAsync(string id);
        Task CreateAsync(BotConfig newBotConfig);
        Task UpdateAsync(string id, BotConfig updatedBotConfig);
        Task RemoveAsync(string id);
    }
}