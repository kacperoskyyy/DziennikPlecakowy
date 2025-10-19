using DziennikPlecakowy.DTO;

namespace DziennikPlecakowy.Interfaces;

// Interfejs serwisu uwierzytelniania
public interface IAuthService
{
    Task<bool> RegisterAsync(UserRegisterRequestDTO request);
    Task<AuthResponseDTO?> Login(UserAuthRequestDTO userAuthData);
    Task<AuthResponseDTO?> RefreshTokenAsync(string refreshToken);
}
