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
    public class MountainCreateRequest
    {
        public string Name { get; set; }

        public double Height { get; set; }

        public GeoPoint Location { get; set; }

        public string Description { get; set; }

    }
}
