using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SocialNetwork.Core.Models
{
    public class Reaction
    {
        // Посилання на користувача, який відреагував
        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }

        [BsonElement("type")]
        public string Type { get; set; } // "like", "love", "haha"

        [BsonRepresentation(BsonType.DateTime)]
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
