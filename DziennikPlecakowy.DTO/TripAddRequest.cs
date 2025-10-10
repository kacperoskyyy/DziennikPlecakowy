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
        public DateTime TripDate { get; set; }
        public double Distance { get; set; }
        public double Duration { get; set; }
        public GeoPoint[] GeoPointList { get; set; }
        public string Name { get; set; }
        public double ElevationGain { get; set; }
        public int Steps { get; set; }

    }
}
