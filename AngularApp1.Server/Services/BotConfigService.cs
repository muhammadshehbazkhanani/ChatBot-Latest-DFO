using AngularApp1.Server.Models.Entities;
using AngularApp1.Server.Repositories.Interfaces;
using AngularApp1.Server.Services.Interfaces;

namespace AngularApp1.Server.Services
{
    public class BotConfigService : IBotConfigService
    {
        private readonly IBotConfigRepository _repository;

        public BotConfigService(IBotConfigRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<BotConfig>> GetAsync() =>
            await _repository.GetAllAsync();

        public async Task<BotConfig?> GetAsync(string id) =>
            await _repository.GetByIdAsync(id);

        public async Task CreateAsync(BotConfig newBotConfig) =>
            await _repository.CreateAsync(newBotConfig);

        public async Task UpdateAsync(string id, BotConfig updatedBotConfig) =>
            await _repository.UpdateAsync(id, updatedBotConfig);

        public async Task RemoveAsync(string id) =>
            await _repository.DeleteAsync(id);
    }
}
