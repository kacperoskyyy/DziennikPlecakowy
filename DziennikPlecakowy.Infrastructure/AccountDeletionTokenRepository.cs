using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.Models;
using MongoDB.Driver;

namespace DziennikPlecakowy.Infrastructure
{
    public class AccountDeletionTokenRepository : IAccountDeletionTokenRepository
    {
        private readonly IMongoCollection<AccountDeletionToken> _tokens;

        public AccountDeletionTokenRepository(IMongoDbContext dbContext)
        {
            _tokens = dbContext.AccountDeletionTokens;
        }

        public Task AddAsync(AccountDeletionToken token)
        {
            return _tokens.InsertOneAsync(token);
        }

        public Task<AccountDeletionToken?> GetByHashedTokenAsync(string hashedToken)
        {
            var filter = Builders<AccountDeletionToken>.Filter.Eq(t => t.TokenHash, hashedToken);
            return _tokens.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _tokens.DeleteOneAsync(t => t.Id == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }

        public async Task<bool> DeleteAllByUserIdAsync(string userId)
        {
            var result = await _tokens.DeleteManyAsync(t => t.UserId == userId);
            return result.IsAcknowledged;
        }
    }
}