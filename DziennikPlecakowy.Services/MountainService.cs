using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.Models;
using DziennikPlecakowy.Shared;

namespace DziennikPlecakowy.Services
{
    public class MountainService : IMountainService
    {
        private readonly DziennikPlecakowyDbContext _context;

        // Konstruktor
        public MountainService(DziennikPlecakowyDbContext context)
        {
            _context = context;
        }

        // Pobieranie góry po id
        public Mountain? GetMountainById(string id)
        {
            try
            {
                var mountain = _context.Mountains.Find(m => m.Id == id).FirstOrDefault();
                return mountain;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        // Pobieranie wszystkich gór
        public List<Mountain> GetMountains()
        {
            try
            {
                return _context.Mountains.Find(_ => true).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<Mountain>();
            }
        }

        // Dodawanie góry
        public int AddMountain(Mountain mountain)
        {
            try
            {
                _context.Mountains.InsertOne(mountain);
                return 1; // Zwróć 1, aby wskazać sukces
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }
        }

        // Aktualizacja góry
        public int UpdateMountain(Mountain mountain)
        {
            try
            {
                var result = _context.Mountains.ReplaceOne(m => m.Id == mountain.Id, mountain);
                return result.ModifiedCount > 0 ? 1 : -1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }
        }

        // Usuwanie góry
        public int DeleteMountain(string id)
        {
            try
            {
                var result = _context.Mountains.DeleteOne(m => m.Id == id);
                return result.DeletedCount > 0 ? 1 : -1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }
        }
    }
}
