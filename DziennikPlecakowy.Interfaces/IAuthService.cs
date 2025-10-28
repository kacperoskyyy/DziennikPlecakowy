using DziennikPlecakowy.DTO;

namespace DziennikPlecakowy.Interfaces;

public interface IAuthService
{
    Task<bool> RegisterAsync(UserRegisterRequestDTO request);
    Task<AuthResponseDTO?> Login(UserAuthRequestDTO userAuthData);
    Task<AuthResponseDTO?> RefreshTokenAsync(string refreshToken);
}
