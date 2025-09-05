using DziennikPlecakowy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DziennikPlecakowy.DTO
{
    public class AuthData
    {
        public string UserId { get; set; } 
        public string Username { get; set; }
        public string Email { get; set; }
        public ICollection<UserRole> Roles { get; set; }
        public string Token { get; set; }

        public AuthData(string id, string username, string email, ICollection<UserRole> userRoles, string token)
        {
            UserId = id;
            Username = username;
            Email = email;
            Roles = userRoles;
            Token = token;
        }
    }
}
