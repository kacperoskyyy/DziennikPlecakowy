using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DziennikPlecakowy.Models
{
    public class GeoPoint
    {
        [BsonElement("latitude")]
        public double Latitude { get; set; }

        [BsonElement("longitude")]
        public double Longitude { get; set; }
        [BsonElement("height")]
        public double Height { get; set; }
    }
}
