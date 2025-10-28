using DziennikPlecakowy.Models;

namespace DziennikPlecakowy.Interfaces;

public interface IUserStatRepository
{
    Task<UserStat?> GetByUserIdAsync(string userId);
    Task AddAsync(UserStat stats);
    Task<bool> UpdateAsync(UserStat stats);
    Task<bool> DeleteAsync(string userId);
}