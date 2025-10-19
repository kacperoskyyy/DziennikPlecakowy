using DziennikPlecakowy.Data;
using DziennikPlecakowy.Models.Local;
using SQLite;

namespace DziennikPlecakowy.Services.Local;

public class DatabaseService
{
    private SQLiteAsyncConnection _database;

    public SQLiteAsyncConnection GetConnection()
    {
        if (_database == null)
        {
            _database = new SQLiteAsyncConnection(DatabaseConstants.DatabasePath, DatabaseConstants.Flags);
        }
        return _database;
    }


    public async Task InitializeDatabaseAsync()
    {
        var db = GetConnection();

        await db.CreateTableAsync<LocalTrip>();
        await db.CreateTableAsync<LocalGeoPoint>();
        await db.CreateTableAsync<LocalRefreshToken>();
    }
}