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

        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public List<string> Interests { get; set; } = new List<string>();

        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> Friends { get; set; } = new List<string>();

        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> Followers { get; set; } = new List<string>();

        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> Following { get; set; } = new List<string>();
    }
}