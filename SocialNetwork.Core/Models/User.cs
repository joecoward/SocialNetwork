using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace SocialNetwork.Core.Models
{
    [BsonIgnoreExtraElements]
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("password")]
        public string PasswordHash { get; set; }

        [BsonElement("firstName")]
        public string FirstName { get; set; }

        [BsonElement("lastName")]
        public string LastName { get; set; }

        [BsonElement("interests")]
        public List<string> Interests { get; set; } = new List<string>();

        [BsonElement("friends")]
        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> Friends { get; set; } = new List<string>();

        [BsonElement("followers")]
        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> Followers { get; set; } = new List<string>();

        [BsonElement("following")]
        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> Following { get; set; } = new List<string>();
    }
}