using DziennikPlecakowy.Models;
using MongoDB.Driver;

namespace DziennikPlecakowy.Interfaces;

public interface IMongoDbContext
{
    IMongoCollection<User> Users { get; }
    IMongoCollection<Trip> Trips { get; }
    IMongoCollection<UserStat> UserStats { get; }
    IMongoCollection<RefreshToken> RefreshTokens { get; }
    IMongoCollection<PasswordResetToken> PasswordResetTokens { get; }
}
