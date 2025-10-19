using DziennikPlecakowy.Models.Local;
using DziennikPlecakowy.Services.Local;
using SQLite;

namespace DziennikPlecakowy.Repositories
{
    public class TokenRepository
    {
        private readonly SQLiteAsyncConnection _db;

        public TokenRepository(DatabaseService dbService)
        {
            _db = dbService.GetConnection();
        }

        public Task SaveTokenAsync(LocalRefreshToken token)
        {
            return _db.InsertOrReplaceAsync(token);
        }

        public Task<LocalRefreshToken> GetTokenAsync()
        {
            return _db.Table<LocalRefreshToken>().FirstOrDefaultAsync(t => t.Id == 1);
        }

        public Task DeleteTokenAsync()
        {
            return _db.Table<LocalRefreshToken>().DeleteAsync(t => t.Id == 1);
        }
    }
}