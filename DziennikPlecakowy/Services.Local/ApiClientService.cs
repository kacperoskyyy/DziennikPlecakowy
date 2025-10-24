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

    private const string BaseApiUrl = "https://localhost:7046";

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

    public async Task<HttpResponseMessage> GetAsync(string requestUri)
    {
        var response = await _httpClient.GetAsync(requestUri);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            // Jeśli 401, spróbuj odświeżyć token i ponowić żądanie
            bool refreshed = await HandleUnauthorizedResponseAsync();
            if (refreshed)
            {
                // Ponów żądanie z nowym tokenem
                response = await _httpClient.GetAsync(requestUri);
            }
        }
        return response;
    }

    public async Task<HttpResponseMessage> PostAsJsonAsync<T>(string requestUri, T value)
    {
        var response = await _httpClient.PostAsJsonAsync(requestUri, value);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            bool refreshed = await HandleUnauthorizedResponseAsync();
            if (refreshed)
            {
                // Ponów żądanie
                response = await _httpClient.PostAsJsonAsync(requestUri, value);
            }
        }
        return response;
    }


    private async Task<bool> HandleUnauthorizedResponseAsync()
    {
        await _tokenRefreshLock.WaitAsync();
        try
        {

            if (_isRefreshingToken)
            {
                return true;
            }

            _isRefreshingToken = true;

            var localToken = await _tokenRepository.GetTokenAsync();
            if (localToken == null || string.IsNullOrEmpty(localToken.Token))
            {
                return false;
            }

            var refreshRequest = new RefreshTokenRequestDTO { RefreshToken = localToken.Token };
            var response = await _httpClient.PostAsJsonAsync("/api/Auth/refresh", refreshRequest);

            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDTO>();

                localToken.Token = authResponse.RefreshToken;
                localToken.ExpiryDate = DateTime.UtcNow.AddDays(7);
                await _tokenRepository.SaveTokenAsync(localToken);

                SetAccessToken(authResponse.Token);
                return true;
            }

            return false;
        }
        finally
        {
            _isRefreshingToken = false;
            _tokenRefreshLock.Release();
        }
    }
    public Task<HttpResponseMessage> PostRawAsync<T>(string requestUri, T value)
    {
        return _httpClient.PostAsJsonAsync(requestUri, value);
    }
    public async Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content = null)
    {
        var response = await _httpClient.PutAsync(requestUri, content);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            bool refreshed = await HandleUnauthorizedResponseAsync();
            if (refreshed)
            {
                // Ponów żądanie z nowym tokenem
                response = await _httpClient.PutAsync(requestUri, content);
            }
        }
        return response;
    }
}