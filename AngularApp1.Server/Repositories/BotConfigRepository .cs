using AngularApp1.Server.Models.Entities;
using AngularApp1.Server.Repositories.Interfaces;
using MongoDB.Driver;

namespace AngularApp1.Server.Repositories
{
    public class BotConfigRepository : IBotConfigRepository
    {
        private readonly IMongoCollection<BotConfig> _collection;

        public BotConfigRepository(IMongoCollection<BotConfig> collection)
        {
            _collection = collection;
        }

        public async Task<List<BotConfig>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<BotConfig> GetByIdAsync(string id)
        {
            return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(BotConfig config)
        {
            await _collection.InsertOneAsync(config);
        }

        public async Task UpdateAsync(string id, BotConfig config)
        {
            await _collection.ReplaceOneAsync(x => x.Id == id, config);
        }

        public async Task DeleteAsync(string id)
        {
            await _collection.DeleteOneAsync(x => x.Id == id);
        }
    }
}
