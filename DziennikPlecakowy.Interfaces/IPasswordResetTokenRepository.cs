using DziennikPlecakowy.Models;

namespace DziennikPlecakowy.Interfaces;

public interface IPasswordResetTokenRepository
{
    Task AddAsync(PasswordResetToken token);
    Task<PasswordResetToken?> GetByHashedTokenAsync(string hashedToken);
    Task DeleteAsync(string id);
    Task DeleteAllByUserIdAsync(string userId);
}