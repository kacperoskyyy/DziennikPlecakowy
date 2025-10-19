using DziennikPlecakowy.Models;

namespace DziennikPlecakowy.Interfaces;

// Interfejs repozytorium statystyk użytkownika
public interface IUserStatRepository
{
    Task<UserStat?> GetByUserIdAsync(string userId);
    Task AddAsync(UserStat stats);
    Task<bool> UpdateAsync(UserStat stats);
    Task<bool> DeleteAsync(string userId);
}