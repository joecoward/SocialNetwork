using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SocialNetwork.Core.Models
{
    public class Comment
    {

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("commentId")]
        public string CommentId { get; set; }

        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }

        [BsonElement("text")]
        public string Text { get; set; }

        [BsonRepresentation(BsonType.DateTime)]
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("reactions")]
        public List<Reaction> Reactions { get; set; } = new List<Reaction>();
    }
}
