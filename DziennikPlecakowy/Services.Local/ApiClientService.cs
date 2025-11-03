using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Repositories;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace DziennikPlecakowy.Services.Local;

public class ApiClientService
{
    private readonly HttpClient _httpClient;
    private readonly TokenRepository _tokenRepository;

    private const string BaseApiUrl = "http://10.0.2.2:5101";

    private string _currentAccessToken;
    private bool _isRefreshingToken = false;
    private static readonly SemaphoreSlim _tokenRefreshLock = new SemaphoreSlim(1, 1);

    public ApiClientService(TokenRepository tokenRepository)
    {
        _tokenRepository = tokenRepository;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(BaseApiUrl)
        };
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public void SetAccessToken(string token)
    {
        _currentAccessToken = token;
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public void ClearAccessToken()
    {
        _currentAccessToken = null;
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    // Dodaliśmy 'handleUnauthorized = true'
    public async Task<HttpResponseMessage> GetAsync(string requestUri, bool handleUnauthorized = true)
    {
        var response = await _httpClient.GetAsync(requestUri);

        // Obsługuj 401 tylko jeśli flaga jest ustawiona
        if (handleUnauthorized && response.StatusCode == HttpStatusCode.Unauthorized)
        {
            bool refreshed = await HandleUnauthorizedResponseAsync();
            if (refreshed)
            {
                response = await _httpClient.GetAsync(requestUri); // Ponów z nowym tokenem
            }
        }
        return response;
    }

    // Dodaliśmy 'handleUnauthorized = true'
    public async Task<HttpResponseMessage> PostAsJsonAsync<T>(string requestUri, T value, bool handleUnauthorized = true)
    {
        var response = await _httpClient.PostAsJsonAsync(requestUri, value);

        if (handleUnauthorized && response.StatusCode == HttpStatusCode.Unauthorized)
        {
            bool refreshed = await HandleUnauthorizedResponseAsync();
            if (refreshed)
            {
                response = await _httpClient.PostAsJsonAsync(requestUri, value); // Ponów
            }
        }
        return response;
    }

    // Dodaliśmy 'handleUnauthorized = true'
    public async Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content = null, bool handleUnauthorized = true)
    {
        var response = await _httpClient.PutAsync(requestUri, content);

        if (handleUnauthorized && response.StatusCode == HttpStatusCode.Unauthorized)
        {
            bool refreshed = await HandleUnauthorizedResponseAsync();
            if (refreshed)
            {
                response = await _httpClient.PutAsync(requestUri, content); // Ponów
            }
        }
        return response;
    }

    // Usunęliśmy PostRawAsync - nie jest już potrzebna

    private async Task<bool> HandleUnauthorizedResponseAsync()
    {
        await _tokenRefreshLock.WaitAsync();
        try
        {
            if (_isRefreshingToken)
            {
                // Inny wątek już odświeża token, zakładamy sukces
                return true;
            }

            _isRefreshingToken = true;

            var localToken = await _tokenRepository.GetTokenAsync();
            if (localToken == null || string.IsNullOrEmpty(localToken.Token))
            {
                return false; // Brak refresh tokena, nie można odświeżyć
            }

            var refreshRequest = new RefreshTokenRequestDTO { RefreshToken = localToken.Token };

            // POPRAWKA: Wywołaj PostAsJsonAsync z handleUnauthorized: false
            // aby zapobiec nieskończonej pętli, jeśli refresh token wygaśnie
            var response = await PostAsJsonAsync("/api/Auth/refresh", refreshRequest, handleUnauthorized: false);

            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDTO>();

                localToken.Token = authResponse.RefreshToken;
                localToken.ExpiryDate = DateTime.UtcNow.AddDays(7);
                await _tokenRepository.SaveTokenAsync(localToken);

                SetAccessToken(authResponse.Token);
                return true;
            }

            return false; // Odświeżanie się nie powiodło
        }
        finally
        {
            _isRefreshingToken = false;
            _tokenRefreshLock.Release();
        }
    }
}