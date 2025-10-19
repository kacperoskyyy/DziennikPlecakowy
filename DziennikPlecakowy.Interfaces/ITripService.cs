using DziennikPlecakowy.Models;

namespace DziennikPlecakowy.Interfaces;

// Interfejs serwisu zarządzania wycieczkami
public interface ITripService
{
    Task<Trip> AddTripAsync(Trip trip);
    Task<bool> UpdateTripAsync(Trip trip, string userId);
    Task<bool> DeleteTripAsync(string tripId, string userId);
    Task<IEnumerable<Trip>> GetUserTripsAsync(string userId);
}