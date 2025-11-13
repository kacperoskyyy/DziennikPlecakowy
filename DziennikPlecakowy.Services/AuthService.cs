using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.Models;
using System.Security.Cryptography;

namespace DziennikPlecakowy.Services;

public class AuthService : IAuthService
{
    private readonly IUserService _userService;
    private readonly ICypherService _cypherService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly IEmailService _emailService;
    private readonly IHashService _hashService;

    public AuthService(
        IUserService userService,
        ICypherService cypherService,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IEmailService emailService,
        IHashService hashService)
    {
        _userService = userService;
        _cypherService = cypherService;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _emailService = emailService;
        _hashService = hashService;
    }
    public async Task RegisterAsync(UserRegisterRequestDTO request)
    {

        await _userService.UserRegister(request);
    }
    public async Task<AuthResponseDTO?> Login(UserAuthRequestDTO userAuthData)
    {
        var user = await _userService.GetUserByEmail(userAuthData.Email);

        if (user == null || !_userService.CheckPassword(user, userAuthData.Password))
        {
            return null;
        }

        if(user.IsBlocked)
        {
            return new AuthResponseDTO
            {
                Token = "LOCKED",
                RefreshToken = "LOCKED",
                MustChangePassword = false
            };
        }

        await _refreshTokenRepository.DeleteAllByUserIdAsync(user.Id);

        var jwtToken = _cypherService.GenerateJwtToken(user);
        var refreshToken = await CreateAndStoreRefreshToken(user.Id);

        await _userService.SetLastLogin(user.Id);

        return new AuthResponseDTO
        {
            Token = jwtToken,
            RefreshToken = refreshToken.Token,
            MustChangePassword = user.MustChangePassword
        };
    }
    public async Task<AuthResponseDTO?> RefreshTokenAsync(string token)
    {
        var storedToken = await _refreshTokenRepository.GetByTokenAsync(token);

        if (storedToken == null || storedToken.ExpiryDate <= DateTime.UtcNow)
        {
            return null;
        }

        var user = await _userService.GetUserById(storedToken.UserId);
        if (user == null)
        {
            return null;
        }
        if (user.IsBlocked)
        {
            return new AuthResponseDTO
            {
                Token = "LOCKED",
                RefreshToken = "LOCKED",
                MustChangePassword = false
            };
        }

        var newJwtToken = _cypherService.GenerateJwtToken(user);
        var newRefreshToken = await CreateAndStoreRefreshToken(user.Id);

        await _refreshTokenRepository.DeleteAsync(storedToken.Id);

        return new AuthResponseDTO
        {
            Token = newJwtToken,
            RefreshToken = newRefreshToken.Token
        };
    }
    private async Task<RefreshToken> CreateAndStoreRefreshToken(string userId)
    {
        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = GenerateRefreshTokenString(),
            ExpiryDate = DateTime.UtcNow.AddDays(1)
        };

        await _refreshTokenRepository.AddAsync(refreshToken);
        return refreshToken;
    }
    private string GenerateRefreshTokenString()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
    public async Task LogoutAsync(string token)
    {
        var storedToken = await _refreshTokenRepository.GetByTokenAsync(token);

        if (storedToken != null)
        {
            await _refreshTokenRepository.DeleteAsync(storedToken.Id);
        }
    }

    public async Task<bool> RequestPasswordResetAsync(string email)
    { 
        var user = await _userService.GetUserByEmail(email);

        if (user == null)
        {
            return true;
        }

        await _passwordResetTokenRepository.DeleteAllByUserIdAsync(user.Id);

        var plainTextToken = new Random().Next(100000, 999999).ToString();
        var hashedToken = _hashService.Hash(plainTextToken);

        var expiryTime = DateTime.UtcNow.AddMinutes(15);

        var resetToken = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = hashedToken,
            ExpiryDate = expiryTime,
            ExpireAt = expiryTime
        };

        await _passwordResetTokenRepository.AddAsync(resetToken);

        await _emailService.SendPasswordResetEmailAsync(user.Email, plainTextToken);

        return true;
    }

    public async Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        var hashedToken = _hashService.Hash(token);

        var storedToken = await _passwordResetTokenRepository.GetByHashedTokenAsync(hashedToken);

        if (storedToken == null || storedToken.ExpiryDate <= DateTime.UtcNow)
        {
            return false;
        }

        var success = await _userService.ResetPasswordAsync(storedToken.UserId, newPassword);

        if (success)
        {
 
            await _passwordResetTokenRepository.DeleteAsync(storedToken.Id);
        }
        return success;
    }
}