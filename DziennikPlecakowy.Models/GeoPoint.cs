using MongoDB.Bson.Serialization.Attributes;

namespace DziennikPlecakowy.Models;

// Model reprezentujący punkt geograficzny z dodatkowymi informacjami
public class GeoPoint
{
    [BsonElement("latitude")]
    public double Latitude { get; set; }

    [BsonElement("longitude")]
    public double Longitude { get; set; }
    [BsonElement("height")]
    public double Height { get; set; }
    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; }
}
