using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.Models;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace DziennikPlecakowy.Infrastructure;

public class UserStatRepository : IUserStatRepository
{
    private readonly IMongoCollection<UserStat> _userStats;
    public UserStatRepository(IMongoDbContext dbContext)
    {
        _userStats = dbContext.UserStats;
    }
    public Task AddAsync(UserStat stats)
    {
        return _userStats.InsertOneAsync(stats);
    }
    public Task<UserStat?> GetByUserIdAsync(string userId)
    {
        var filter = Builders<UserStat>.Filter.Eq(s => s.UserId, userId);
        return _userStats.Find(filter).FirstOrDefaultAsync();
    }
    public async Task<bool> UpdateAsync(UserStat stats)
    {
        ReplaceOneResult result = await _userStats.ReplaceOneAsync(
            s => s.UserId == stats.UserId,
            stats
        );
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }
    public async Task<bool> DeleteAsync(string userId)
    {
        var result = await _userStats.DeleteOneAsync(s => s.UserId == userId);
        return result.IsAcknowledged && result.DeletedCount > 0;
    }
}