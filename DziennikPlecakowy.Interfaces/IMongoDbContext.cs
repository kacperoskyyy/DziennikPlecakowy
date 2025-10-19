using DziennikPlecakowy.Models;
using MongoDB.Driver;

namespace DziennikPlecakowy.Interfaces;

// Interfejs kontekstu bazy danych MongoDB
public interface IMongoDbContext
{
    IMongoCollection<User> Users { get; }
    IMongoCollection<Trip> Trips { get; }
    IMongoCollection<UserStat> UserStats { get; }
    IMongoCollection<RefreshToken> RefreshTokens { get; }
}
