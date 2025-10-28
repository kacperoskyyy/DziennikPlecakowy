using DziennikPlecakowy.Models.Local;
using DziennikPlecakowy.Services.Local;
using SQLite;

namespace DziennikPlecakowy.Repositories;
// Repozytorium do zarządzania lokalnym tokenem odświeżania
public class TokenRepository
{
    private readonly SQLiteAsyncConnection _db;
    private readonly DatabaseService _dbService;

    public TokenRepository(DatabaseService dbService)
    {
        _db = dbService.GetConnection();
        _dbService = dbService;
    }

    public async Task SaveTokenAsync(LocalRefreshToken token)
    {
        await _dbService.InitializeDatabaseAsync();
        await _db.InsertOrReplaceAsync(token);
    }

    public async Task<LocalRefreshToken> GetTokenAsync()
    {
        await _dbService.InitializeDatabaseAsync();
        return await _db.Table<LocalRefreshToken>().FirstOrDefaultAsync(t => t.Id == 1);
    }

    public async Task DeleteTokenAsync()
    {
        await _dbService.InitializeDatabaseAsync();
        await _db.Table<LocalRefreshToken>().DeleteAsync(t => t.Id == 1);
    }
}