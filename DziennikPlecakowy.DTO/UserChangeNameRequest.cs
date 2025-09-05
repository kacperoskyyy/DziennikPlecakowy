using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DziennikPlecakowy.DTO
{
    public class UserChangeNameRequest
    {
        public string UserId { get; set; }
        public string NewUsername { get; set; }
    }
}
