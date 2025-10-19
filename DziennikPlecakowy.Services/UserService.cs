using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.Models;

namespace DziennikPlecakowy.Services;

// Serwis zarządzania użytkownikami
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserStatRepository _userStatRepository;
    private readonly ITripRepository _tripRepository;
    private readonly IHashService _hash;
    private readonly ICypherService _cypherService;
    // Konstruktor serwisu użytkowników
    public UserService(IUserRepository userRepository, IUserStatRepository userStatRepository, ITripRepository tripRepository, IHashService hash, ICypherService cypherService)
    {
        _userRepository = userRepository;
        _userStatRepository = userStatRepository;
        _tripRepository = tripRepository;
        _hash = hash;
        _cypherService = cypherService;
    }
    // Odszyfrowywanie danych użytkownika
    private void DecryptUser(User user)
    {
        if (user == null) return;
        user.Email = _cypherService.Decrypt(user.Email);
        user.Username = _cypherService.Decrypt(user.Username);
    }
    // Szyfrowanie i zapisywanie danych użytkownika
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

    // --- Zarządzanie Kontem ---
    // Zmiana nazwy użytkownika
    public async Task<bool> ChangeUsernameAsync(string userId, string newUsername)
    {
        User? user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        user.Username = newUsername;
        return await EncryptAndSaveUserAsync(user);
    }
    // Zmiana adresu email
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
    // Zmiana hasła
    public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        User? user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        if (!CheckPassword(user, currentPassword))
        {
            return false;
        }

        user.HashedPassword = _hash.Hash(newPassword);

        var success = await _userRepository.UpdateAsync(user);
        if (success) DecryptUser(user);
        return success;
    }
    // Usuwanie konta użytkownika
    public async Task<bool> DeleteUserAsync(string userId)
    {
        User? user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        await _userStatRepository.DeleteAsync(userId);
        await _tripRepository.DeleteAllByUserIdAsync(userId);

        return await _userRepository.DeleteAsync(userId);
    }

    // Rejestracja nowego użytkownika
    public async Task<int> UserRegister(UserRegisterRequestDTO userRegister)
    {
        try
        {
            string encryptedEmail = _cypherService.Encrypt(userRegister.Email.ToLower());
            User? existingUser = await _userRepository.GetByEncryptedEmailAsync(encryptedEmail);
            if (existingUser != null) return -1;

            User user = new User
            {
                Email = encryptedEmail,
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

            return 1;
        }
        catch (Exception)
        {
            return -1;
        }
    }
    // Pobieranie użytkownika po adresie email
    public async Task<User?> GetUserByEmail(string email)
    {
        string encryptedEmail = _cypherService.Encrypt(email.ToLower());
        var user = await _userRepository.GetByEncryptedEmailAsync(encryptedEmail);

        if (user == null) return null;

        DecryptUser(user);
        return user;
    }
    // Pobieranie użytkownika po identyfikatorze
    public async Task<User?> GetUserById(string id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return null;

        DecryptUser(user);
        return user;
    }
    // Sprawdzanie poprawności hasła
    public bool CheckPassword(User user, string password)
    {
        return user.HashedPassword == _hash.Hash(password);
    }

    // Aktualizowanie danych użytkownika
    public async Task<int> UpdateUser(User user)
    {
        var result = await _userRepository.UpdateAsync(user);
        return result ? 1 : -1;
    }
    // Usuwanie użytkownika
    public async Task<int> DeleteUser(User user)
    {
        var result = await _userRepository.DeleteAsync(user.Id);
        return result ? 1 : -1;
    }
    // Ustawianie czasu ostatniego logowania
    public async Task<int> SetLastLogin(string Id)
    {
        var user = await GetUserById(Id);
        if (user == null) return 0;
        user.LastLoginTime = DateTime.Now;

        return await EncryptAndSaveUserAsync(user) ? 1 : 0;
    }
    // Nadawanie uprawnień administratora
    public async Task<int> SetAdmin(User user)
    {
        if (!user.Roles.Contains(UserRole.Admin))
        {
            user.Roles.Add(UserRole.Admin);
            return await EncryptAndSaveUserAsync(user) ? 1 : 0;
        }
        return 0;
    }
}