using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System;

namespace DziennikPlecakowy.Services
{
    public class ApiServiceClient
    {
        private readonly HttpClient _http;

        public ApiServiceClient()
        {
            _http = new HttpClient
            {
                BaseAddress = new Uri("https://your.api.url/") //URL API
            };
        }

        public void SetAuthToken(string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _http.DefaultRequestHeaders.Authorization = null;
            }
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            var res = await _http.GetAsync(endpoint);
            if (res.IsSuccessStatusCode) return await res.Content.ReadFromJsonAsync<T>();
            return default;
        }

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            var res = await _http.PostAsJsonAsync(endpoint, data);
            if (res.IsSuccessStatusCode) return await res.Content.ReadFromJsonAsync<TResponse>();
            return default;
        }

        public async Task<bool> PostAsync<TRequest>(string endpoint, TRequest data)
        {
            var res = await _http.PostAsJsonAsync(endpoint, data);
            return res.IsSuccessStatusCode;
        }

        public async Task<bool> PutAsync<TRequest>(string endpoint, TRequest data)
        {
            var res = await _http.PutAsJsonAsync(endpoint, data);
            return res.IsSuccessStatusCode;
        }
    }
}
