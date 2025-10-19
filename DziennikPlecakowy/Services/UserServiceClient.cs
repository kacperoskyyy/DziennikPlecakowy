using System.Threading.Tasks;
using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.DTO;

namespace DziennikPlecakowy.Services
{
    public class UserServiceClient
    {
        private readonly ApiServiceClient _api;
        public UserServiceClient(ApiServiceClient api) { _api = api; }

        public async Task<bool> ChangeNameAsync(UserChangeNameRequestDTO req)
        {
            return await _api.PutAsync("user/changeName", req);
        }

        public async Task<bool> ChangeEmailAsync(UserChangeEmailRequestDTO req)
        {
            return await _api.PutAsync("user/changeEmail", req);
        }

        public async Task<bool> ChangePasswordAsync(UserChangePasswordRequestDTO req)
        {
            return await _api.PutAsync("user/changePassword", req);
        }
    }
}
