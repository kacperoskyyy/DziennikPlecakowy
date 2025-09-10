using System.Threading.Tasks;
using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.DTO;

namespace DziennikPlecakowy.Services
{
    public class UserServiceClient : IUserService
    {
        private readonly ApiServiceClient _api;
        public UserServiceClient(ApiServiceClient api) { _api = api; }

        public async Task<bool> ChangeNameAsync(UserChangeNameRequest req)
        {
            return await _api.PutAsync("user/changeName", req);
        }

        public async Task<bool> ChangeEmailAsync(UserChangeEmailRequest req)
        {
            return await _api.PutAsync("user/changeEmail", req);
        }

        public async Task<bool> ChangePasswordAsync(UserChangePasswordRequest req)
        {
            return await _api.PutAsync("user/changePassword", req);
        }
    }
}
