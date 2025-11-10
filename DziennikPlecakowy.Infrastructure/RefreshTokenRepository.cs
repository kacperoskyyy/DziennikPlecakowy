using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.Models;
using MongoDB.Driver;

namespace DziennikPlecakowy.Infrastructure;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IMongoCollection<RefreshToken> _refreshTokens;
    public RefreshTokenRepository(IMongoDbContext context)
    {
        _refreshTokens = context.RefreshTokens;
    }
    public async Task AddAsync(RefreshToken refreshToken)
    {
        await _refreshTokens.InsertOneAsync(refreshToken);
    }
    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _refreshTokens.Find(rt => rt.Token == token).FirstOrDefaultAsync();
    }
    public async Task DeleteAsync(string tokenId)
    {
        await _refreshTokens.DeleteOneAsync(rt => rt.Id == tokenId);
    }
    public async Task DeleteAllByUserIdAsync(string userId)
    {
        await _refreshTokens.DeleteManyAsync(rt => rt.UserId == userId);
    }
}