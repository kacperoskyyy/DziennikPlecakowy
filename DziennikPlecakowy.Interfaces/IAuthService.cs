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
        public Task<string> Login(UserAuthRequest userAuthData);
        public Task<User> GetUserInfoFromToken(string token);
    }
}
