using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.Models;
using MongoDB.Driver;

namespace DziennikPlecakowy.Infrastructure;

public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly IMongoCollection<PasswordResetToken> _tokens;

    public PasswordResetTokenRepository(IMongoDbContext dbContext)
    {
        _tokens = dbContext.PasswordResetTokens;
    }

    public Task AddAsync(PasswordResetToken token)
    {
        return _tokens.InsertOneAsync(token);
    }

    public Task<PasswordResetToken?> GetByHashedTokenAsync(string hashedToken)
    {
        var filter = Builders<PasswordResetToken>.Filter.Eq(t => t.TokenHash, hashedToken);
        return _tokens.Find(filter).FirstOrDefaultAsync();
    }

    public Task DeleteAsync(string id)
    {
        return _tokens.DeleteOneAsync(t => t.Id == id);
    }

    public Task DeleteAllByUserIdAsync(string userId)
    {
        return _tokens.DeleteManyAsync(t => t.UserId == userId);
    }
}