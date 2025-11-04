using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Models.Local;
using DziennikPlecakowy.Repositories;
using System.Net;
using System.Net.Http.Json;

namespace DziennikPlecakowy.Services.Local;
// Serwis uwierzytelniania uzytkownika
public class AuthService
{
    private readonly ApiClientService _apiClient;
    private readonly TokenRepository _tokenRepository;
    private readonly DatabaseService _dbService;

    private const string UserIdKey = "auth_user_id";
    private const string UserEmailKey = "auth_user_email";
    private const string UserNameKey = "auth_user_name";

    public AuthService(ApiClientService apiClient, TokenRepository tokenRepository, DatabaseService dbService)
    {
        _apiClient = apiClient;
        _tokenRepository = tokenRepository;
        _dbService = dbService;
    }

    public string GetCurrentUserId() => Preferences.Get(UserIdKey, null);

    public async Task<AuthResult> CheckAndRefreshTokenOnStartupAsync()
    {
        await _dbService.InitializeDatabaseAsync();
        var localToken = await _tokenRepository.GetTokenAsync();
        if (localToken == null || string.IsNullOrEmpty(localToken.Token))
        {
            return AuthResult.Fail("Brak tokena.");
        }

        var request = new RefreshTokenRequestDTO { RefreshToken = localToken.Token };

        // POPRAWKA: Używamy PostAsJsonAsync z flagą false
        var response = await _apiClient.PostAsJsonAsync("/api/Auth/refresh", request, handleUnauthorized: false);

        if (!response.IsSuccessStatusCode)
        {
            await LogoutAsync();
            return AuthResult.Fail("Sesja wygasła.");
        }

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDTO>();
        _apiClient.SetAccessToken(authResponse.Token);

        localToken.Token = authResponse.RefreshToken;
        await _tokenRepository.SaveTokenAsync(localToken);

        var userDto = await FetchAndSaveUserDataAsync();
        if (userDto == null)
        {
            return AuthResult.Fail("Nie udało się pobrać danych użytkownika.");
        }

        return AuthResult.Success(authResponse.MustChangePassword);
    }


    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        await _dbService.InitializeDatabaseAsync();
        var request = new UserAuthRequestDTO { Email = email, Password = password };

        // POPRAWKA: Używamy PostAsJsonAsync z flagą false
        var response = await _apiClient.PostAsJsonAsync("/api/Auth/login", request, handleUnauthorized: false);

        if (!response.IsSuccessStatusCode)
        {
            return await ParseErrorResponse(response);
        }

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDTO>();
        _apiClient.SetAccessToken(authResponse.Token);

        var userDto = await FetchAndSaveUserDataAsync();
        if (userDto == null)
        {
            _apiClient.ClearAccessToken();
            return AuthResult.Fail("Błąd pobierania profilu po zalogowaniu.");
        }

        var localToken = new LocalRefreshToken
        {
            Token = authResponse.RefreshToken,
            UserId = userDto.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(30)
        };
        await _tokenRepository.SaveTokenAsync(localToken);

        return AuthResult.Success(authResponse.MustChangePassword);
    }

    public async Task<AuthResult> RegisterAsync(string username, string email, string password)
    {
        var request = new UserRegisterRequestDTO
        {
            Username = username,
            Email = email,
            Password = password
        };

        // POPRAWKA: Używamy PostAsJsonAsync z flagą false
        var response = await _apiClient.PostAsJsonAsync("/api/Auth/register", request, handleUnauthorized: false);

        if (!response.IsSuccessStatusCode)
        {
            return await ParseErrorResponse(response);
        }

        // Oryginalna logika logowania po rejestracji jest OK
        return await LoginAsync(email, password);
    }

    public async Task LogoutAsync()
    {
        _apiClient.ClearAccessToken();
        await _tokenRepository.DeleteTokenAsync();
        Preferences.Clear();
    }

    private async Task<UserProfileDTO> FetchAndSaveUserDataAsync()
    {
        var userResponse = await _apiClient.GetAsync("/api/User/getUserStats");
        if (!userResponse.IsSuccessStatusCode)
        {
            return null;
        }

        var userDto = await userResponse.Content.ReadFromJsonAsync<UserProfileDTO>();

        if (userDto == null || string.IsNullOrEmpty(userDto.Id))
        {
            return null;
        }

        Preferences.Set(UserIdKey, userDto.Id);
        Preferences.Set(UserEmailKey, userDto.Email);
        Preferences.Set(UserNameKey, userDto.Username);

        return userDto;
    }




    private async Task<AuthResult> ParseErrorResponse(HttpResponseMessage response)
    {
        string defaultError = "Nieznany błąd serwera.";

        try
        {
            var errorDto = await response.Content.ReadFromJsonAsync<ErrorResponseDTO>();
            if (errorDto != null && !string.IsNullOrEmpty(errorDto.Message))
            {
                return AuthResult.Fail(errorDto.Message);
            }
        }
        catch { }

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return AuthResult.Fail("Nieprawidłowy login lub hasło.");
        }
        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            return AuthResult.Fail("Konto zablokowane lub brak uprawnień.");
        }

        return AuthResult.Fail(defaultError);
    }
}