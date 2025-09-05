using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DziennikPlecakowy.DTO
{
    public class UserAuthRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
