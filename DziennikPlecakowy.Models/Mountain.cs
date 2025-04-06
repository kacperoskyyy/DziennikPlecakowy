using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DziennikPlecakowy.Models
{
    public class Mountain
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        //[BsonElement("height")]
        //public double Height { get; set; }

        [BsonElement("location")]
        public GeoPoint Location { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        //// Lista dostępnych szlaków na tę górę
        //[BsonElement("trails")]
        //public List<Trail> Trails { get; set; }
    }
}
