using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DziennikPlecakowy.Models;

namespace DziennikPlecakowy.Shared
{
    public class DziennikPlecakowyDbContext
    {
        private readonly IMongoDatabase _database;

        public DziennikPlecakowyDbContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetConnectionString("MongoDB"));
            _database = client.GetDatabase(configuration["MongoDB:DatabaseName"]);
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
        public IMongoCollection<Mountain> Mountains => _database.GetCollection<Mountain>("Mountains");
        public IMongoCollection<Trip> Trips => _database.GetCollection<Trip>("Trips");
        public IMongoCollection<UserStat> UserStats => _database.GetCollection<UserStat>("UserStats");
    }
}
