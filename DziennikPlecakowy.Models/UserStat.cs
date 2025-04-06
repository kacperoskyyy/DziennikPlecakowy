using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace DziennikPlecakowy.Models
{
    public class UserStat
    {
        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }
        [BsonElement("reachedMountains")]
        public int ReachedMountains { get; set; }
        [BsonElement("highestMountain")]
        public Mountain HighestMountain { get; set; }
        [BsonElement("totalDistance")]
        public double TotalDistance { get; set; }

    }
}
