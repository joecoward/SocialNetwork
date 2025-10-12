using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SocialNetwork.Core.Models
{
    public class Reaction
    {
        // Посилання на користувача, який відреагував
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }

        public string Type { get; set; } // "like", "love", "haha"

        [BsonRepresentation(BsonType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
