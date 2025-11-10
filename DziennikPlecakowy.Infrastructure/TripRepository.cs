using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.Models;
using MongoDB.Driver;
namespace DziennikPlecakowy.Infrastructure;

public class TripRepository : ITripRepository
{
    private readonly IMongoCollection<Trip> _trips;
    public TripRepository(IMongoDbContext dbContext)
    {
        _trips = dbContext.Trips;
    }
    public Task AddAsync(Trip trip)
    {
        return _trips.InsertOneAsync(trip);
    }
    public Task<Trip?> GetByIdAsync(string tripId)
    {
        var filter = Builders<Trip>.Filter.Eq(t => t.Id, tripId);
        return _trips.Find(filter).FirstOrDefaultAsync();
    }
    public async Task<IEnumerable<Trip>> GetByUserAsync(string userId)
    { 
        var filter = Builders<Trip>.Filter.Eq(t => t.UserId, userId);

        return await _trips
            .Find(filter)
            .SortByDescending(t => t.TripDate)
            .ToListAsync();
    }
    public async Task<bool> UpdateAsync(Trip trip)
    {
        ReplaceOneResult result = await _trips.ReplaceOneAsync(
            t => t.Id == trip.Id,
            trip
        );
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }
    public async Task<bool> DeleteAsync(string tripId)
    {
        var result = await _trips.DeleteOneAsync(t => t.Id == tripId);
        return result.IsAcknowledged && result.DeletedCount > 0;
    }
    public async Task<bool> DeleteAllByUserIdAsync(string userId)
    {
        var result = await _trips.DeleteManyAsync(t => t.UserId == userId);
        return result.IsAcknowledged;
    }
}