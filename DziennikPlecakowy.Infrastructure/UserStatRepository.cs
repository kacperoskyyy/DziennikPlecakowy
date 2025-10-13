// DziennikPlecakowy.Infrastructure/UserStatRepository.cs
using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.Models;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace DziennikPlecakowy.Infrastructure
{
    // Repozytorium do zarządzania statystykami użytkowników w MongoDB
    public class UserStatRepository : IUserStatRepository
    {
        private readonly IMongoCollection<UserStat> _userStats;
        // Konstruktor przyjmujący kontekst bazy danych
        public UserStatRepository(IMongoDbContext dbContext)
        {
            _userStats = dbContext.UserStats;
        }
        // Dodanie nowych statystyk użytkownika
        public Task AddAsync(UserStat stats)
        {
            return _userStats.InsertOneAsync(stats);
        }
        // Pobranie statystyk użytkownika na podstawie jego ID
        public Task<UserStat?> GetByUserIdAsync(string userId)
        {
            var filter = Builders<UserStat>.Filter.Eq(s => s.UserId, userId);
            return _userStats.Find(filter).FirstOrDefaultAsync();
        }
        // Aktualizacja istniejących statystyk użytkownika
        public async Task<bool> UpdateAsync(UserStat stats)
        {
            ReplaceOneResult result = await _userStats.ReplaceOneAsync(
                s => s.UserId == stats.UserId,
                stats
            );
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }
        // Usunięcie statystyk użytkownika na podstawie jego ID
        public async Task<bool> DeleteAsync(string userId)
        {
            var result = await _userStats.DeleteOneAsync(s => s.UserId == userId);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }
    }
}