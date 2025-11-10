using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DziennikPlecakowy.Models;


public class RefreshToken
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("userId")]
    public string UserId { get; set; }

    [BsonElement("token")]
    public string Token { get; set; }

    [BsonElement("expiryDate")]
    public DateTime ExpiryDate { get; set; }
}