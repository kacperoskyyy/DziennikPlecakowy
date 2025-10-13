// Plik: DziennikPlecakowy.Interfaces/ITripRepository.cs

using DziennikPlecakowy.Models;

namespace DziennikPlecakowy.Interfaces
{
    // Interfejs repozytorium wycieczek
    public interface ITripRepository
    {
        Task AddAsync(Trip trip);
        Task<Trip?> GetByIdAsync(string tripId);
        Task<IEnumerable<Trip>> GetByUserAsync(string userId);
        Task<bool> UpdateAsync(Trip trip);
        Task<bool> DeleteAsync(string tripId);
        Task<bool> DeleteAllByUserIdAsync(string userId);
    }
}