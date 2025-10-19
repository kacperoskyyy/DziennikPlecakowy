using DziennikPlecakowy.Models.Local;
using DziennikPlecakowy.Services.Local;
using SQLite;

namespace DziennikPlecakowy.Repositories;
public class LocalTripRepository
{
    private readonly SQLiteAsyncConnection _db;

    public LocalTripRepository(DatabaseService dbService)
    {
        _db = dbService.GetConnection();
    }

    public Task<List<LocalTrip>> GetLast50TripsAsync()
    {
        return _db.Table<LocalTrip>()
                  .OrderByDescending(t => t.TripDate)
                  .Take(50)
                  .ToListAsync();
    }

    public async Task<List<TripWithGeoPoints>> GetUnsynchronizedTripsAsync()
    {
        var unsyncedTrips = await _db.Table<LocalTrip>()
                                     .Where(t => !t.IsSynchronized)
                                     .ToListAsync();

        var result = new List<TripWithGeoPoints>();
        foreach (var trip in unsyncedTrips)
        {
            result.Add(await GetTripWithGeoPointsAsync(trip.LocalId));
        }
        return result;
    }

    public async Task<TripWithGeoPoints> GetTripWithGeoPointsAsync(long localTripId)
    {
        var trip = await _db.Table<LocalTrip>().FirstOrDefaultAsync(t => t.LocalId == localTripId);
        if (trip == null)
            return null;

        var points = await _db.Table<LocalGeoPoint>()
                              .Where(p => p.LocalTripId == localTripId)
                              .ToListAsync();

        return new TripWithGeoPoints
        {
            Trip = trip,
            GeoPoints = points
        };
    }

    public async Task<long> SaveTripAsync(LocalTrip trip, IEnumerable<LocalGeoPoint> points)
    {
        if (trip.LocalId != 0)
        {
            await _db.UpdateAsync(trip);
        }
        else
        {
            await _db.InsertAsync(trip);
        }

        foreach (var point in points)
        {
            point.LocalTripId = trip.LocalId;
        }

        await _db.InsertAllAsync(points);

        return trip.LocalId;
    }


    public Task AddGeoPointAsync(LocalGeoPoint point)
    {
        return _db.InsertAsync(point);
    }


    public Task MarkTripAsSynchronizedAsync(long localTripId, string serverId)
    {
        return _db.ExecuteAsync(
            "UPDATE trips SET IsSynchronized = ?, ServerId = ? WHERE LocalId = ?",
            true,
            serverId,
            localTripId);
    }


    public async Task DeleteTripAsync(long localTripId)
    {
        await _db.Table<LocalGeoPoint>().DeleteAsync(p => p.LocalTripId == localTripId);

        await _db.Table<LocalTrip>().DeleteAsync(t => t.LocalId == localTripId);
    }
}