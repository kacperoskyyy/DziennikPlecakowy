using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Models;

namespace DziennikPlecakowy.Interfaces;

public interface ITripService
{
    Task<Trip> AddTripAsync(Trip trip);
    Task<bool> UpdateTripAsync(Trip trip, string userId);
    Task<bool> DeleteTripAsync(string tripId, string userId);
    Task<IEnumerable<Trip>> GetUserTripsAsync(string userId);
    Task<IEnumerable<TripSummaryDTO>> GetUserTripSummariesAsync(string userId);
    Task<Trip?> GetTripByIdAsync(string tripId, string userId);
}