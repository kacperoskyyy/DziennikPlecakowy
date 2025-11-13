using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace DziennikPlecakowy.Models;


public class Trip
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; }

    [BsonElement("tripName")]
    public string Name { get; set; }

    [BsonElement("tripDate")]
    public DateTime TripDate { get; set; }

    [BsonElement("distance")]
    public double Distance { get; set; }

    [BsonElement("duration")]
    public double Duration { get; set; }

    [BsonElement("geopointList")]
    public GeoPoint[] GeoPointList { get; set; }

    [BsonElement("elevationGain")]
    public double ElevationGain { get; set; }

    [BsonElement("steps")]
    public int Steps { get; set; }

}
