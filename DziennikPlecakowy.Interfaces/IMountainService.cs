using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DziennikPlecakowy.Models;

namespace DziennikPlecakowy.Interfaces
{
    public interface IMountainService
    {
        public Mountain? GetMountainById(string id);
        public List<Mountain> GetMountains();
        public int AddMountain(Mountain mountain);
        public int UpdateMountain(Mountain mountain);
        public int DeleteMountain(string id);
    }
}
