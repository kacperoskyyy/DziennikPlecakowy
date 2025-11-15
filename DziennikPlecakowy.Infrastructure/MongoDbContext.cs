using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using DziennikPlecakowy.Models;
using DziennikPlecakowy.Interfaces;

namespace DziennikPlecakowy.Infrastructure;
public class MongoDbContext : IMongoDbContext
{
    private readonly IMongoDatabase _database;
    public MongoDbContext(IConfiguration configuration)
    {
        var client = new MongoClient(configuration["MongoDB:ConnectionString"]);
        _database = client.GetDatabase(configuration["MongoDB:DatabaseName"]);
    }
    public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
    public IMongoCollection<Trip> Trips => _database.GetCollection<Trip>("Trips");
    public IMongoCollection<UserStat> UserStats => _database.GetCollection<UserStat>("UserStats");
    public IMongoCollection<RefreshToken> RefreshTokens => _database.GetCollection<RefreshToken>("RefreshTokens");
    public IMongoCollection<PasswordResetToken> PasswordResetTokens => _database.GetCollection<PasswordResetToken>("PasswordResetTokens");
    public IMongoCollection<AccountDeletionToken> AccountDeletionTokens => _database.GetCollection<AccountDeletionToken>("AccountDeletionTokens");
}
