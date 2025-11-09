using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.Models;
using System.Linq;

namespace DziennikPlecakowy.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserStatRepository _userStatRepository;
    private readonly ITripRepository _tripRepository;
    private readonly IHashService _hash;
    private readonly ICypherService _cypherService;

    public UserService(IUserRepository userRepository, IUserStatRepository userStatRepository, ITripRepository tripRepository, IHashService hash, ICypherService cypherService)
    {
        _userRepository = userRepository;
        _userStatRepository = userStatRepository;
        _tripRepository = tripRepository;
        _hash = hash;
        _cypherService = cypherService;
    }

    private void DecryptUser(User user)
    {
        if (user == null) return;
        user.Email = _cypherService.Decrypt(user.Email);
        user.Username = _cypherService.Decrypt(user.Username);
    }

    private async Task<bool> EncryptAndSaveUserAsync(User user)
    {
        user.Email = _cypherService.Encrypt(user.Email.ToLower());
        user.Username = _cypherService.Encrypt(user.Username);

        var success = await _userRepository.UpdateAsync(user);

        if (success)
        {
            DecryptUser(user);
        }

        return success;
    }


    public async Task<bool> ChangeUsernameAsync(string userId, string newUsername)
    {
        User? user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        user.Username = newUsername;
        return await EncryptAndSaveUserAsync(user);
    }

    public async Task<bool> ChangeEmailAsync(string userId, string newEmail)
    {
        User? user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        string encryptedNewEmail = _cypherService.Encrypt(newEmail.ToLower());
        User? existingUser = await _userRepository.GetByEncryptedEmailAsync(encryptedNewEmail);

        if (existingUser != null && existingUser.Id != userId)
        {
            return false;
        }

        user.Email = newEmail;
        return await EncryptAndSaveUserAsync(user);
    }

    public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        User? user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        if (user.MustChangePassword == false)
        {
            if (!CheckPassword(user, currentPassword))
            {
                return false;
            }
        }

        string newPasswordHash = _hash.Hash(newPassword);

        if (user.HashedPassword == newPasswordHash  || user.PasswordHashesHistory.Contains(newPasswordHash) )
        {
            return false;
        }

        string oldPasswordHash = user.HashedPassword;

        user.HashedPassword = newPasswordHash;
        user.MustChangePassword = false;

        if (!string.IsNullOrEmpty(oldPasswordHash))
        {
            user.PasswordHashesHistory.Add(oldPasswordHash);
        }

        if (user.PasswordHashesHistory.Count > 3)
        {
            user.PasswordHashesHistory.Remove(user.PasswordHashesHistory.First());
        }


        var success = await _userRepository.UpdateAsync(user);
        if (success) DecryptUser(user);
        return success;
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        User? user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        await _userStatRepository.DeleteAsync(userId);
        await _tripRepository.DeleteAllByUserIdAsync(userId);

        return await _userRepository.DeleteAsync(userId);
    }


    public async Task UserRegister(UserRegisterRequestDTO userRegister)
    {
        User user = new User
        {
            Email = _cypherService.Encrypt(userRegister.Email.ToLower()),
            Username = _cypherService.Encrypt(userRegister.Username),
            HashedPassword = _hash.Hash(userRegister.Password),
            CreatedTime = DateTime.Now,
            Roles = { UserRole.User }
        };

        await _userRepository.AddAsync(user);

        UserStat userStat = new UserStat
        {
            UserId = user.Id,
            TripsCount = 0,
            TotalDistance = 0,
            TotalDuration = 0,
            TotalElevationGain = 0,
            TotalSteps = 0
        };
        await _userStatRepository.AddAsync(userStat);
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        string encryptedEmail = _cypherService.Encrypt(email.ToLower());
        var user = await _userRepository.GetByEncryptedEmailAsync(encryptedEmail);

        if (user == null) return null;

        DecryptUser(user);
        return user;
    }

    public async Task<User?> GetUserById(string id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return null;

        DecryptUser(user);
        return user;
    }

    public bool CheckPassword(User user, string password)
    {
        return user.HashedPassword == _hash.Hash(password);
    }


    public async Task<int> UpdateUser(User user)
    {
        var result = await _userRepository.UpdateAsync(user);
        return result ? 1 : -1;
    }

    public async Task<int> DeleteUser(User user)
    {
        var result = await _userRepository.DeleteAsync(user.Id);
        return result ? 1 : -1;
    }

    public async Task<int> SetLastLogin(string Id)
    {
        var user = await GetUserById(Id);
        if (user == null) return 0;
        user.LastLoginTime = DateTime.Now;

        return await EncryptAndSaveUserAsync(user) ? 1 : 0;
    }

    public async Task<int> SetAdmin(User user)
    {
        if (!user.Roles.Contains(UserRole.Admin))
        {
            user.Roles.Add(UserRole.Admin);
            return await EncryptAndSaveUserAsync(user) ? 1 : 0;
        }
        return 0;
    }

    public async Task<UserStat> GetUserStats(string userId)
    {
        var stats = await _userStatRepository.GetByUserIdAsync(userId);
        if (stats == null)
        {
            stats = new UserStat
            {
                UserId = userId,
                TripsCount = 0,
                TotalDistance = 0,
                TotalDuration = 0,
                TotalElevationGain = 0,
                TotalSteps = 0
            };
            await _userStatRepository.AddAsync(stats);
        }
        return stats;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        foreach (var user in users)
        {
            DecryptUser(user);
        }
        return users;
    }

    public async Task<bool> SetUserLockStatusAsync(string userId, bool isLocked)
    {
        User? user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        user.IsBlocked = isLocked;
        return await _userRepository.UpdateAsync(user);
    }

    public async Task<bool> SetPasswordChangeStatusAsync(string userId, bool mustChange)
    {
        User? user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        user.MustChangePassword = mustChange;
        return await _userRepository.UpdateAsync(user);
    }

    public async Task<bool> SetUserRoleAsync(string userId, UserRole newRole)
    {
        User? user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        if (newRole == UserRole.Admin && !user.Roles.Contains(UserRole.Admin))
        {
            user.Roles.Add(UserRole.Admin);
        }
        else if (newRole == UserRole.User && user.Roles.Contains(UserRole.Admin))
        {
            user.Roles.Remove(UserRole.Admin);
        }

        return await _userRepository.UpdateAsync(user);
    }
}