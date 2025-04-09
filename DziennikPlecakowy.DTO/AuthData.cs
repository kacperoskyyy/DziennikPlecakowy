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
        public bool IsAdmin { get; set; }
        public bool IsSuperUser { get; set; }
        public AuthData(string id, string username, string email, bool isAdmin, bool isSuperUser)
        {
            UserId = id;
            Username = username;
            Email = email;
            IsAdmin = isAdmin;
            IsSuperUser = isSuperUser;
        }
    }
}
