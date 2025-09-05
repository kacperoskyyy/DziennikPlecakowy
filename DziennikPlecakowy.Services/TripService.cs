using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DziennikPlecakowy.Models;
using DziennikPlecakowy.Shared;
using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.DTO;
using MongoDB.Driver;

namespace DziennikPlecakowy.Services
{
    public class TripService : ITripService
    {
        private readonly DziennikPlecakowyDbContext _context;

        // Konstruktor
        public TripService(DziennikPlecakowyDbContext context)
        {
            _context = context;
        }

        // Dodawanie wycieczki
        public async Task<int> AddTrip(Trip trip)
        {
            try
            {
                await _context.Trips.InsertOneAsync(trip);
                return 1; // Zwróć 1, aby wskazać sukces
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
                return -1;
            }
        }

        // Aktualizacja wycieczki
        public async Task<int> UpdateTrip(Trip trip)
        {
            try
            {
                var result = await _context.Trips.ReplaceOneAsync(t => t.Id == trip.Id, trip);
                return result.ModifiedCount > 0 ? 1 : -1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
                return -1;
            }
        }

        // Usuwanie wycieczki
        public async Task<int> DeleteTrip(string tripId)
        {
            try
            {
                var result = await _context.Trips.DeleteOneAsync(t => t.Id == tripId);
                return result.DeletedCount > 0 ? 1 : -1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
                return -1;
            }
        }

        // Wycieczki użytkownika
        public async Task<List<Trip>> GetUserTrips(AuthData auth)
        {
            try
            {
                return await _context.Trips.Find(t => t.UserId == auth.UserId).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
                return null;
            }
        }
    }
}
