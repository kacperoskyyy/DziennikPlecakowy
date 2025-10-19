using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Models.Local;
using DziennikPlecakowy.Repositories;
using System.Net.Http.Json;

namespace DziennikPlecakowy.Services.Local;

public class AuthService
{
    private readonly ApiClientService _apiClient;
    private readonly TokenRepository _tokenRepository;

    private const string UserIdKey = "auth_user_id";
    private const string UserEmailKey = "auth_user_email";
    private const string UserNameKey = "auth_user_name";

    public AuthService(ApiClientService apiClient, TokenRepository tokenRepository)
    {
        _apiClient = apiClient;
        _tokenRepository = tokenRepository;
    }

    public string GetCurrentUserId() => Preferences.Get(UserIdKey, null);

    public async Task<bool> CheckAndRefreshTokenOnStartupAsync()
    {
        var localToken = await _tokenRepository.GetTokenAsync();
        if (localToken == null || string.IsNullOrEmpty(localToken.Token))
        {
            return false;
        }

        var request = new RefreshTokenRequestDTO { RefreshToken = localToken.Token };

        var response = await _apiClient.PostRawAsync("/api/Auth/refresh", request);

        if (!response.IsSuccessStatusCode)
        {

            await LogoutAsync();
            return false;
        }

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDTO>();

        _apiClient.SetAccessToken(authResponse.Token);

        localToken.Token = authResponse.RefreshToken;
        await _tokenRepository.SaveTokenAsync(localToken);

        var userDto = await FetchAndSaveUserDataAsync();
        return (userDto != null);
    }


    public async Task<bool> LoginAsync(string email, string password)
    {
        var request = new UserAuthRequestDTO { Email = email, Password = password };

        var response = await _apiClient.PostRawAsync("/api/Auth/login", request);

        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDTO>();

        _apiClient.SetAccessToken(authResponse.Token);


        var userDto = await FetchAndSaveUserDataAsync();
        if (userDto == null)
        {
            _apiClient.ClearAccessToken();
            return false;
        }

        var localToken = new LocalRefreshToken
        {
            Token = authResponse.RefreshToken,
            UserId = userDto.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(30)
        };
        await _tokenRepository.SaveTokenAsync(localToken);

        return true;
    }

    public async Task<bool> RegisterAsync(string username, string email, string password)
    {
        var request = new UserRegisterRequestDTO
        {
            Username = username,
            Email = email,
            Password = password
        };

        var response = await _apiClient.PostRawAsync("/api/Auth/register", request);

        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        return await LoginAsync(email, password);
    }

    public async Task LogoutAsync()
    {
        _apiClient.ClearAccessToken();
        await _tokenRepository.DeleteTokenAsync();
        Preferences.Clear();
    }


    private async Task<UserDetailDTO> FetchAndSaveUserDataAsync()
    {
        var userResponse = await _apiClient.GetAsync("/api/User/getUserStats");
        if (!userResponse.IsSuccessStatusCode)
        {
            return null;
        }

        var userDto = await userResponse.Content.ReadFromJsonAsync<UserDetailDTO>();

        if (userDto == null || string.IsNullOrEmpty(userDto.Id))
        {
            return null;
        }

        Preferences.Set(UserIdKey, userDto.Id);
        Preferences.Set(UserEmailKey, userDto.Email);
        Preferences.Set(UserNameKey, userDto.Username);

        return userDto;
    }
}