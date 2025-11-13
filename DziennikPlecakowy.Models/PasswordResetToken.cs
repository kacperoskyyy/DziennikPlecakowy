using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;

namespace DziennikPlecakowy.Models;

public class PasswordResetToken
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("userId")]
    public string UserId { get; set; }

    [BsonElement("tokenHash")]
    public string TokenHash { get; set; }

    [BsonElement("expiryDate")]
    public DateTime ExpiryDate { get; set; }

    [BsonElement("expireAt")]
    public DateTime ExpireAt { get; set; }
}