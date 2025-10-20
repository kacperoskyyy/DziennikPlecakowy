using DziennikPlecakowy.Models;

namespace DziennikPlecakowy.Interfaces;

// Interfejs repozytorium użytkowników
public interface IUserRepository
{
    Task AddAsync(User user);
    Task<User?> GetByIdAsync(string id);
    Task<User?> GetByEncryptedEmailAsync(string encryptedEmail);
    Task<bool> UpdateAsync(User user);
    Task<bool> DeleteAsync(string id);
    Task<List<User>> GetAllAsync();
}