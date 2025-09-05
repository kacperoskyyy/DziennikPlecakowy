using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DziennikPlecakowy.Interfaces
{
    public interface ICypherService
    {
        public string Encrypt(string text);
        public string Decrypt(string text);
        public string GenerateJwtToken(string Id);
        public ClaimsPrincipal ValidateJwtToken(string token);
    }
}
