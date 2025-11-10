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


    private string _currentAccessToken;
    private bool _isRefreshingToken = false;
    private static readonly SemaphoreSlim _tokenRefreshLock = new SemaphoreSlim(1, 1);

    public ApiClientService(HttpClient httpClient, TokenRepository tokenRepository)
    {
        _tokenRepository = tokenRepository;
        _httpClient = httpClient;
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

    public async Task<HttpResponseMessage> GetAsync(string requestUri, bool handleUnauthorized = true)
    {
        var response = await _httpClient.GetAsync(requestUri);

        if (handleUnauthorized && response.StatusCode == HttpStatusCode.Unauthorized)
        {
            bool refreshed = await HandleUnauthorizedResponseAsync();
            if (refreshed)
            {
                response = await _httpClient.GetAsync(requestUri);
            }
        }
        return response;
    }

    public async Task<HttpResponseMessage> PostAsJsonAsync<T>(string requestUri, T value, bool handleUnauthorized = true)
    {
        var response = await _httpClient.PostAsJsonAsync(requestUri, value);

        if (handleUnauthorized && response.StatusCode == HttpStatusCode.Unauthorized)
        {
            bool refreshed = await HandleUnauthorizedResponseAsync();
            if (refreshed)
            {
                response = await _httpClient.PostAsJsonAsync(requestUri, value);
            }
        }
        return response;
    }

    public async Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content = null, bool handleUnauthorized = true)
    {
        var response = await _httpClient.PutAsync(requestUri, content);

        if (handleUnauthorized && response.StatusCode == HttpStatusCode.Unauthorized)
        {
            bool refreshed = await HandleUnauthorizedResponseAsync();
            if (refreshed)
            {
                response = await _httpClient.PutAsync(requestUri, content);
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

            var response = await PostAsJsonAsync("/api/Auth/refresh", refreshRequest, handleUnauthorized: false);

            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDTO>();

                localToken.Token = authResponse.RefreshToken;
                localToken.ExpiryDate = DateTime.UtcNow.AddDays(1);
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

    public async Task<HttpResponseMessage> DeleteAsync(string requestUri, bool handleUnauthorized = true)
    {
        var response = await _httpClient.DeleteAsync(requestUri);

        if (handleUnauthorized && response.StatusCode == HttpStatusCode.Unauthorized)
        {
            bool refreshed = await HandleUnauthorizedResponseAsync();
            if (refreshed)
            {
                response = await _httpClient.DeleteAsync(requestUri);
            }
        }
        return response;
    }


}