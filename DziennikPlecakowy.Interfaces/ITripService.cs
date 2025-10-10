using System.Collections.Generic;
using System.Threading.Tasks;
using DziennikPlecakowy.Models;

namespace DziennikPlecakowy.Interfaces
{
    public interface ITripService
    {
        Task<bool> AddTripAsync(Trip trip);
        Task<bool> UpdateTripAsync(Trip trip, string userId);
        Task<bool> DeleteTripAsync(string tripId, string userId);
        Task<IEnumerable<Trip>> GetUserTripsAsync(string userId);
    }
}