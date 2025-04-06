using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DziennikPlecakowy.DTO;

namespace DziennikPlecakowy.Interfaces
{
    public interface IAuthService
    {
        public Task<AuthData> Login(UserAuthRequest userAuthData);
    }
}
