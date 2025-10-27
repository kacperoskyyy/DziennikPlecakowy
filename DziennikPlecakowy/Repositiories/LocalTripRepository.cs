using DziennikPlecakowy.Models.Local;
using DziennikPlecakowy.Services.Local;
using SQLite;

namespace DziennikPlecakowy.Repositories;
public class LocalTripRepository
{
    private readonly SQLiteAsyncConnection _db;
    private readonly DatabaseService _dbService;

    public LocalTripRepository(DatabaseService dbService)
    {
        _dbService = dbService;
        _db = dbService.GetConnection();
    }

    public async Task<List<LocalTrip>> GetLast50TripsAsync()
    {
        await _dbService.InitializeDatabaseAsync();
        return await _db.Table<LocalTrip>()
                      .OrderByDescending(t => t.TripDate)
                      .Take(50)
                      .ToListAsync();
    }

    public async Task<List<TripWithGeoPoints>> GetUnsynchronizedTripsAsync()
    {
        await _dbService.InitializeDatabaseAsync();
        var unsyncedTrips = await _db.Table<LocalTrip>()
                                         .Where(t => !t.IsSynchronized)
                                         .ToListAsync();

        var result = new List<TripWithGeoPoints>();
        foreach (var trip in unsyncedTrips)
        {
            var tripWithPoints = await GetTripWithGeoPointsAsync(trip.LocalId);
            if (tripWithPoints != null)
            {
                result.Add(tripWithPoints);
            }
        }
        return result;
    }

    public async Task<TripWithGeoPoints> GetTripWithGeoPointsAsync(long localTripId)
    {
        await _dbService.InitializeDatabaseAsync();
        var trip = await _db.Table<LocalTrip>().FirstOrDefaultAsync(t => t.LocalId == localTripId);
        if (trip == null)
            return null;

        var points = await _db.Table<LocalGeoPoint>()
                                  .Where(p => p.LocalTripId == localTripId)
                                  .ToListAsync();

        return new TripWithGeoPoints
        {
            Trip = trip,
            GeoPoints = points ?? new List<LocalGeoPoint>()
        };
    }

    public async Task<long> SaveTripAsync(LocalTrip trip, IEnumerable<LocalGeoPoint> points)
    {
        await _dbService.InitializeDatabaseAsync();
        if (trip.LocalId != 0)
        {
            await _db.UpdateAsync(trip);
        }
        else
        {
            await _db.InsertAsync(trip);
        }

        var pointsList = points?.ToList() ?? new List<LocalGeoPoint>();

        foreach (var point in pointsList)
        {
            point.LocalTripId = trip.LocalId;
        }

        if (pointsList.Any())
        {
            await _db.InsertAllAsync(pointsList);
        }

        return trip.LocalId;
    }


    public async Task AddGeoPointAsync(LocalGeoPoint point)
    {
        await _dbService.InitializeDatabaseAsync();
        await _db.InsertAsync(point);
    }


    public async Task MarkTripAsSynchronizedAsync(long localTripId, string serverId)
    {
        await _dbService.InitializeDatabaseAsync();
        await _db.ExecuteAsync(
            "UPDATE trips SET IsSynchronized = ?, ServerId = ? WHERE LocalId = ?",
            true,
            serverId,
            localTripId);
    }


    public async Task DeleteTripAsync(long localTripId)
    {
        await _dbService.InitializeDatabaseAsync();

        await _db.RunInTransactionAsync(conn => { 
            conn.Execute("DELETE FROM geo_points WHERE LocalTripId = ?", localTripId);
            conn.Execute("DELETE FROM trips WHERE LocalId = ?", localTripId);
        });
    }
}