using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DziennikPlecakowy.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("userName")]
        public string Username { get; set; }

        [BsonElement("passwordHash")]
        public string HashedPassword { get; set; }

        [BsonElement("createdTime")]
        public DateTime CreatedTime { get; set; }

        [BsonElement("lastLoginTime")]
        public DateTime? LastLoginTime { get; set; }

        [BsonElement("isAdmin")]
        public bool IsAdmin { get; set; }
        [BsonElement("isSuperUser")]
        public bool IsSuperUser { get; set; }
        
    }
}
