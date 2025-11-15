using DziennikPlecakowy.DTO;

namespace DziennikPlecakowy.Interfaces;

public interface IAuthService
{
    Task RegisterAsync(UserRegisterRequestDTO request);
    Task<AuthResponseDTO?> Login(UserAuthRequestDTO userAuthData);
    Task<AuthResponseDTO?> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(string token);
    Task<bool> RequestPasswordResetAsync(string email);
    Task<bool> ResetPasswordAsync(string token, string newPassword);
    Task<bool> RequestAccountDeletionAsync(string userId);
    Task<bool> ConfirmAccountDeletionAsync(string userId, string token);
}
