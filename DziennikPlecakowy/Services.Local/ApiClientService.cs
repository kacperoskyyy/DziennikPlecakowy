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
    }

    public void ClearAccessToken()
    {
        _currentAccessToken = null;
    }

    public async Task<HttpResponseMessage> GetAsync(string requestUri, bool handleUnauthorized = true)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        if (!string.IsNullOrEmpty(_currentAccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _currentAccessToken);
        }

        request.Headers.CacheControl = new CacheControlHeaderValue
        {
            NoCache = true,
            NoStore = true,
            MustRevalidate = true
        };
        request.Headers.Pragma.ParseAdd("no-cache");

        var response = await _httpClient.SendAsync(request);

        if (handleUnauthorized && response.StatusCode == HttpStatusCode.Unauthorized)
        {
            bool refreshed = await HandleUnauthorizedResponseAsync();
            if (refreshed)
            {
                var retryRequest = new HttpRequestMessage(HttpMethod.Get, requestUri);

                if (!string.IsNullOrEmpty(_currentAccessToken))
                {
                    retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _currentAccessToken);
                }

                retryRequest.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    NoStore = true,
                    MustRevalidate = true
                };
                retryRequest.Headers.Pragma.ParseAdd("no-cache");

                response = await _httpClient.SendAsync(retryRequest);
            }
        }
        return response;
    }

    public async Task<HttpResponseMessage> PostAsJsonAsync<T>(string requestUri, T value, bool handleUnauthorized = true)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        request.Content = JsonContent.Create(value);

        if (!string.IsNullOrEmpty(_currentAccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _currentAccessToken);
        }

        var response = await _httpClient.SendAsync(request);

        if (handleUnauthorized && response.StatusCode == HttpStatusCode.Unauthorized)
        {
            bool refreshed = await HandleUnauthorizedResponseAsync();
            if (refreshed)
            {
                var retryRequest = new HttpRequestMessage(HttpMethod.Post, requestUri);
                retryRequest.Content = JsonContent.Create(value);

                if (!string.IsNullOrEmpty(_currentAccessToken))
                {
                    retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _currentAccessToken);
                }
                response = await _httpClient.SendAsync(retryRequest);
            }
        }
        return response;
    }

    public async Task<HttpResponseMessage> PutAsJsonAsync<T>(string requestUri, T value, bool handleUnauthorized = true)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, requestUri);
        request.Content = JsonContent.Create(value);

        if (!string.IsNullOrEmpty(_currentAccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _currentAccessToken);
        }

        var response = await _httpClient.SendAsync(request);

        if (handleUnauthorized && response.StatusCode == HttpStatusCode.Unauthorized)
        {
            bool refreshed = await HandleUnauthorizedResponseAsync();
            if (refreshed)
            {
                var retryRequest = new HttpRequestMessage(HttpMethod.Put, requestUri);
                retryRequest.Content = JsonContent.Create(value);

                if (!string.IsNullOrEmpty(_currentAccessToken))
                {
                    retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _currentAccessToken);
                }
                response = await _httpClient.SendAsync(retryRequest);
            }
        }
        return response;
    }

    public async Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content = null, bool handleUnauthorized = true)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, requestUri);
        request.Content = content;

        if (!string.IsNullOrEmpty(_currentAccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _currentAccessToken);
        }

        var response = await _httpClient.SendAsync(request);

        if (handleUnauthorized && response.StatusCode == HttpStatusCode.Unauthorized)
        {
            bool refreshed = await HandleUnauthorizedResponseAsync();
            if (refreshed)
            {
                var retryRequest = new HttpRequestMessage(HttpMethod.Put, requestUri);
                retryRequest.Content = content;

                if (!string.IsNullOrEmpty(_currentAccessToken))
                {
                    retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _currentAccessToken);
                }
                response = await _httpClient.SendAsync(retryRequest);
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
                localToken.ExpiryDate = DateTime.UtcNow.AddDays(1);
                await _tokenRepository.SaveTokenAsync(localToken);

                SetAccessToken(authResponse.Token);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Błąd w HandleUnauthorizedResponseAsync: {ex.Message}");
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
        var request = new HttpRequestMessage(HttpMethod.Delete, requestUri);

        if (!string.IsNullOrEmpty(_currentAccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _currentAccessToken);
        }

        var response = await _httpClient.SendAsync(request);

        if (handleUnauthorized && response.StatusCode == HttpStatusCode.Unauthorized)
        {
            bool refreshed = await HandleUnauthorizedResponseAsync();
            if (refreshed)
            {
                var retryRequest = new HttpRequestMessage(HttpMethod.Delete, requestUri);

                if (!string.IsNullOrEmpty(_currentAccessToken))
                {
                    retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _currentAccessToken);
                }
                response = await _httpClient.SendAsync(retryRequest);
            }
        }
        return response;
    }
}