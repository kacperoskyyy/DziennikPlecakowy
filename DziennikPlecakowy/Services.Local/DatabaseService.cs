using DziennikPlecakowy.Data;
using DziennikPlecakowy.Models.Local;
using SQLite;

namespace DziennikPlecakowy.Services.Local;

public class DatabaseService
{
    private SQLiteAsyncConnection _database;

    private readonly Lazy<SQLiteAsyncConnection> _lazyInitializer = new Lazy<SQLiteAsyncConnection>(() =>
    {
        return new SQLiteAsyncConnection(DatabaseConstants.DatabasePath, DatabaseConstants.Flags);
    });

    private SQLiteAsyncConnection Database => _lazyInitializer.Value;

    public SQLiteAsyncConnection GetConnection()
    {
        return Database;
    }

    public async Task InitializeDatabaseAsync()
    {
        var db = GetConnection();

        await db.CreateTableAsync<LocalTrip>();
        await db.CreateTableAsync<LocalGeoPoint>();
        await db.CreateTableAsync<LocalRefreshToken>();
    }
}