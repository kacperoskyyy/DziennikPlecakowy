using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Models;

namespace DziennikPlecakowy.Interfaces
{
    public interface ITripService
    {
        public Task<int> AddTrip(Trip trip);
        public Task<int> UpdateTrip(Trip trip);
        public Task<int> DeleteTrip(string tripId);
        public Task<List<Trip>> GetUserTrips(string token);
    }
}
