using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DziennikPlecakowy.Models;

namespace DziennikPlecakowy.DTO
{
    public class TripAddRequest
    {
        public string UserId { get; set; }

        public DateTime TripDate { get; set; }
        public string MountainId { get; set; }

        public double Distance { get; set; }

        public double Duration { get; set; }

        public GeoPoint[] GeopointList { get; set; }

    }
}
