using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SocialNetwork.Core.Models
{
    public class Comment
    {
        
        public string CommentId { get; set; }

        
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }

        public string Text { get; set; }

        [BsonRepresentation(BsonType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<Reaction> Reactions { get; set; } = new List<Reaction>();
    }
}
