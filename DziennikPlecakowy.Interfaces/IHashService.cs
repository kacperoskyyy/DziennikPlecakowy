using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DziennikPlecakowy.Interfaces
{
    // Interfejs serwisu haszowania
    public interface IHashService
    {
        public string Hash(string input);
    }
}
