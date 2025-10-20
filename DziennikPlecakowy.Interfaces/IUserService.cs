using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Models;

namespace DziennikPlecakowy.Interfaces;

public interface IUserService
{
    Task<int> UserRegister(UserRegisterRequestDTO userRegister);
    Task<User?> GetUserByEmail(string email);
    Task<User?> GetUserById(string id);
    bool CheckPassword(User user, string password);
    Task<int> UpdateUser(User user);
    Task<int> DeleteUser(User user);
    Task<bool> DeleteUserAsync(string userId);
    Task<int> SetLastLogin(string Id);
    Task<int> SetAdmin(User user);
    Task<UserStat> GetUserStats(string userId);
    Task<bool> ChangeUsernameAsync(string userId, string newUsername);
    Task<bool> ChangeEmailAsync(string userId, string newEmail);
    Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<bool> SetUserLockStatusAsync(string userId, bool isLocked);
    Task<bool> SetPasswordChangeStatusAsync(string userId, bool mustChange);
    Task<bool> SetUserRoleAsync(string userId, UserRole newRole);
}
