using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Models;

namespace DziennikPlecakowy.Interfaces
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(UserRegisterRequest request);
        Task<AuthResponse?> Login(UserAuthRequest userAuthData);
        Task<AuthResponse?> RefreshTokenAsync(string refreshToken);
    }
}
