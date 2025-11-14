using DziennikPlecakowy.Data;
using DziennikPlecakowy.Models.Local;
using SQLite;

namespace DziennikPlecakowy.Services.Local;

public class DatabaseService
{
    private Task _initializationTask = null;
    private bool _isInitialized = false;
    private readonly Lazy<SQLiteAsyncConnection> _lazyInitializer = new Lazy<SQLiteAsyncConnection>(() =>
    {
        System.Diagnostics.Debug.WriteLine("[DatabaseService] Initializing SQLite connection...");
        try
        {
            var connection = new SQLiteAsyncConnection(DatabaseConstants.DatabasePath, DatabaseConstants.Flags);
            System.Diagnostics.Debug.WriteLine("[DatabaseService] SQLite connection initialized.");
            return connection;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DatabaseService] CRITICAL ERROR initializing connection: {ex}");
            throw;
        }
    });

    private SQLiteAsyncConnection Database => _lazyInitializer.Value;

    public SQLiteAsyncConnection GetConnection()
    {
        try
        {
            return Database;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DatabaseService] CRITICAL ERROR getting connection: {ex}");
            throw;
        }
    }

 
    public async Task InitializeDatabaseAsync()
    {
        if (_isInitialized) return;

        if (_initializationTask != null)
        {
            await _initializationTask;
            return;
        }

        _initializationTask = InitializeInternalAsync();
        await _initializationTask;
        _isInitialized = true;
        _initializationTask = null;

    }
    private async Task InitializeInternalAsync()
    {
        SQLiteAsyncConnection db = null;
        try
        {
            db = GetConnection();
            System.Diagnostics.Debug.WriteLine("[DatabaseService] Got connection. Creating tables...");

            await db.CreateTableAsync<LocalTrip>();
            System.Diagnostics.Debug.WriteLine("[DatabaseService] Table 'LocalTrip' created/checked.");

            await db.CreateTableAsync<LocalGeoPoint>();
            System.Diagnostics.Debug.WriteLine("[DatabaseService] Table 'LocalGeoPoint' created/checked.");

            await db.CreateTableAsync<LocalRefreshToken>();
            System.Diagnostics.Debug.WriteLine("[DatabaseService] Table 'LocalRefreshToken' created/checked.");

            System.Diagnostics.Debug.WriteLine("[DatabaseService] All tables checked/created successfully.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DatabaseService] CRITICAL ERROR during InitializeDatabaseAsync: {ex}");
            _initializationTask = null;
            _isInitialized = false;
            throw new Exception($"Failed to initialize database tables: {ex.Message}", ex);
        }
    }

    public async Task CloseConnectionAsync()
    {
        if (_lazyInitializer.IsValueCreated)
        {
            await Database.CloseAsync();
            _isInitialized = false;
        }
    }

    public async Task DeleteDatabaseFileAsync()
    {
        await CloseConnectionAsync();

        if (File.Exists(DatabaseConstants.DatabasePath))
        {
            try
            {
                File.Delete(DatabaseConstants.DatabasePath);
                System.Diagnostics.Debug.WriteLine("[DatabaseService] Database file deleted.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DatabaseService] ERROR deleting database file: {ex.Message}");
            }
        }
    }
}