using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Models.Local;
using DziennikPlecakowy.Services.Local;
using SQLite;

namespace DziennikPlecakowy.Repositories;

public class LocalTripRepository
{
    private readonly SQLiteAsyncConnection _db;
    private readonly DatabaseService _dbService;
    private readonly AuthService _authService;

    public LocalTripRepository(DatabaseService dbService, AuthService authService)
    {
        _dbService = dbService;
        _db = dbService.GetConnection();
        _authService = authService;
    }

    public async Task<List<LocalTrip>> GetTripsForUserAsync()
    {
        await _dbService.InitializeDatabaseAsync();
        var userId = _authService.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId)) return new List<LocalTrip>();

        return await _db.Table<LocalTrip>()
                      .Where(t => t.UserId == userId)
                      .OrderByDescending(t => t.TripDate)
                      .ToListAsync();
    }

    public async Task UpsertTripFromServerAsync(TripSummaryDTO summary)
    {
        await _dbService.InitializeDatabaseAsync();
        var userId = _authService.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId)) return;

        var existingTrip = await _db.Table<LocalTrip>()
                                    .FirstOrDefaultAsync(t => t.ServerId == summary.Id && t.UserId == userId);

        if (existingTrip != null)
        {
            existingTrip.Name = summary.Name;
            existingTrip.TripDate = summary.TripDate;
            existingTrip.Distance = summary.Distance;
            existingTrip.IsSynchronized = true;
            await _db.UpdateAsync(existingTrip);
        }
        else
        {
            var newTrip = new LocalTrip
            {
                ServerId = summary.Id,
                UserId = userId,
                Name = summary.Name,
                TripDate = summary.TripDate,
                Distance = summary.Distance,
                IsSynchronized = true,
            };
            await _db.InsertAsync(newTrip);
        }
    }

    public async Task<List<TripWithGeoPoints>> GetUnsynchronizedTripsAsync()
    {
        await _dbService.InitializeDatabaseAsync();
        var userId = _authService.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId)) return new List<TripWithGeoPoints>();

        var unsyncedTrips = await _db.Table<LocalTrip>()
                                         .Where(t => !t.IsSynchronized && t.UserId == userId)
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
        var userId = _authService.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId)) return null;

        var trip = await _db.Table<LocalTrip>().FirstOrDefaultAsync(t => t.LocalId == localTripId && t.UserId == userId);
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

        var userId = _authService.GetCurrentUserId();
        var trip = await _db.Table<LocalTrip>().FirstOrDefaultAsync(t => t.LocalId == localTripId && t.UserId == userId);

        if (trip != null)
        {
            await _db.RunInTransactionAsync(conn => {
                conn.Execute("DELETE FROM geo_points WHERE LocalTripId = ?", localTripId);
                conn.Execute("DELETE FROM trips WHERE LocalId = ?", localTripId);
            });
        }
    }
    public async Task DeletaAllTrips()
    {
        await _dbService.InitializeDatabaseAsync();
        await _db.ExecuteAsync("DELETE FROM geo_points");
        await _db.ExecuteAsync("DELETE FROM trips");
    }
}