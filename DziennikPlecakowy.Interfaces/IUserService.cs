using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Models;

namespace DziennikPlecakowy.Interfaces;

// Interfejs serwisu zarządzania użytkownikami
public interface IUserService
{
    Task<int> UserRegister(UserRegisterRequestDTO userRegister);
    Task<User?> GetUserByEmail(string email);
    Task<User?> GetUserById(string id);
    Task<int> SetLastLogin(string Id);
    Task<int> SetAdmin(User user);
    bool CheckPassword(User user, string password);
    Task<bool> DeleteUserAsync(string userId);
    Task<bool> ChangeEmailAsync(string userId, string newEmail);
    Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    Task<bool> ChangeUsernameAsync(string userId, string newUsername);
    Task<int> UpdateUser(User user);
    Task<int> DeleteUser(User user);
    Task<UserStat> GetUserStats(string userId);
}