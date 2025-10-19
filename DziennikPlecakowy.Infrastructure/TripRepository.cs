using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.Models;
using MongoDB.Driver;
namespace DziennikPlecakowy.Infrastructure;

// Implementacja repozytorium TripRepository
public class TripRepository : ITripRepository
{
    private readonly IMongoCollection<Trip> _trips;
    // Konstruktor przyjmujący kontekst bazy danych
    public TripRepository(IMongoDbContext dbContext)
    {
        _trips = dbContext.Trips;
    }
    // Dodanie nowej wycieczki
    public Task AddAsync(Trip trip)
    {
        return _trips.InsertOneAsync(trip);
    }
    // Pobranie wycieczki po jej identyfikatorze
    public Task<Trip?> GetByIdAsync(string tripId)
    {
        var filter = Builders<Trip>.Filter.Eq(t => t.Id, tripId);
        return _trips.Find(filter).FirstOrDefaultAsync();
    }
    // Pobranie wszystkich wycieczek danego użytkownika
    public async Task<IEnumerable<Trip>> GetByUserAsync(string userId)
    { 
        var filter = Builders<Trip>.Filter.Eq(t => t.UserId, userId);

        return await _trips
            .Find(filter)
            .SortByDescending(t => t.TripDate)
            .ToListAsync();
    }
    // Aktualizacja wycieczki
    public async Task<bool> UpdateAsync(Trip trip)
    {
        ReplaceOneResult result = await _trips.ReplaceOneAsync(
            t => t.Id == trip.Id,
            trip
        );
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }
    // Usunięcie wycieczki po jej identyfikatorze
    public async Task<bool> DeleteAsync(string tripId)
    {
        var result = await _trips.DeleteOneAsync(t => t.Id == tripId);
        return result.IsAcknowledged && result.DeletedCount > 0;
    }
    // Usunięcie wszystkich wycieczek danego użytkownika
    public async Task<bool> DeleteAllByUserIdAsync(string userId)
    {
        var result = await _trips.DeleteManyAsync(t => t.UserId == userId);
        return result.IsAcknowledged;
    }
}