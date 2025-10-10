// DziennikPlecakowy.Infrastructure/UserRepository.cs
using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.Models;
using MongoDB.Driver;

namespace DziennikPlecakowy.Infrastructure
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _users;

        public UserRepository(IMongoDbContext dbContext)
        {
            _users = dbContext.Users;
        }

        public Task AddAsync(User user)
        {
            return _users.InsertOneAsync(user);
        }

        public Task<User?> GetByIdAsync(string id)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, id);
            return _users.Find(filter).FirstOrDefaultAsync();
        }

        public Task<User?> GetByEmailAsync(string email)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Email, email.ToLower());
            return _users.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateAsync(User user)
        {
            ReplaceOneResult result = await _users.ReplaceOneAsync(
                u => u.Id == user.Id,
                user
            );
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _users.DeleteOneAsync(u => u.Id == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }

        public Task<User?> GetByEncryptedEmailAsync(string encryptedEmail)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Email, encryptedEmail);
            return _users.Find(filter).FirstOrDefaultAsync();
        }
    }
}