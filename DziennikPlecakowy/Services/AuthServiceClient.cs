using System.Threading.Tasks;
using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.DTO;
using Microsoft.Maui.Storage;

namespace DziennikPlecakowy.Services
{
    public class AuthServiceClient
    {
        private readonly ApiServiceClient _api;

        public AuthServiceClient(ApiServiceClient api)
        {
            _api = api;
        }

        public async Task<AuthData> LoginAsync(UserAuthRequest request)
        {
            var result = await _api.PostAsync<UserAuthRequest, AuthData>("auth/login", request);
            if (result != null)
            {
                // zapisz token w SecureStorage
                await SecureStorage.Default.SetAsync("auth_token", result.Token);
                _api.SetAuthToken(result.Token);
            }
            return result;
        }

        public async Task<bool> RegisterAsync(UserRegisterRequest request)
        {
            return await _api.PostAsync("auth/register", request);
        }

        public async Task LogoutAsync()
        {
            SecureStorage.Default.Remove("auth_token");
            _api.SetAuthToken(null);
        }
    }
}
