using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.Models;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace DziennikPlecakowy.Infrastructure
{
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
    }
}