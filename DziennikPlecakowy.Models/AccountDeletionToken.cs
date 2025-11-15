using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DziennikPlecakowy.Models
{
    public class AccountDeletionToken
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }

        public string TokenHash { get; set; }

        public DateTime ExpiryDate { get; set; }

        [BsonElement("ExpireAt")]
        public DateTime ExpireAt { get; set; }
    }
}