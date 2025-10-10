using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace DziennikPlecakowy.Models
{
    public class UserStat
    {
        [BsonId]
        public string Id { get; set; }
        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }
        [BsonElement("tripsCount")]
        public int TripsCount { get; set; }

        [BsonElement("totalDistance")]
        public double TotalDistance { get; set; }
        [BsonElement("totalDuration")]
        public double TotalDuration { get; set; }
        [BsonElement("totalElevationGain")]
        public double TotalElevationGain { get; set; }
        [BsonElement("TotalSteps")]
        public long TotalSteps { get; set; }

    }
}
