using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AngularApp1.Server.Models.Entities
{
    public class BotConfig
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("AppName")]
        public string AppName { get; set; } = null!;

        [BsonElement("Config1")]
        public string Config1 { get; set; } = null!;

        [BsonElement("Config2")]
        public string Config2 { get; set; } = null!;

        [BsonElement("Config3")]
        public string Config3 { get; set; } = null!;

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}