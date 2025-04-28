using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AngularApp1.Server.Models.Entities
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Password { get; set; } = string.Empty;
        public string Salt { get; set; } = default!;
        public string Role { get; set; } = "user";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
