using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.Models;
using MongoDB.Driver;

namespace DziennikPlecakowy.Infrastructure;

// Implementacja repozytorium UserRepository
public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _users;

    // Konstruktor przyjmujący kontekst bazy danych
    public UserRepository(IMongoDbContext dbContext)
    {
        _users = dbContext.Users;
    }
    // Dodanie nowego użytkownika
    public Task AddAsync(User user)
    {
        return _users.InsertOneAsync(user);
    }
    // Pobranie użytkownika po jego identyfikatorze
    public Task<User?> GetByIdAsync(string id)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, id);
        return _users.Find(filter).FirstOrDefaultAsync();
    }
    // Pobranie użytkownika po jego emailu
    public Task<User?> GetByEmailAsync(string email)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Email, email.ToLower());
        return _users.Find(filter).FirstOrDefaultAsync();
    }
    // Aktualizacja użytkownika
    public async Task<bool> UpdateAsync(User user)
    {
        ReplaceOneResult result = await _users.ReplaceOneAsync(
            u => u.Id == user.Id,
            user
        );
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }
    // Usunięcie użytkownika po jego identyfikatorze
    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _users.DeleteOneAsync(u => u.Id == id);
        return result.IsAcknowledged && result.DeletedCount > 0;
    }
    // Pobranie użytkownika po zaszyfrowanym emailu
    public Task<User?> GetByEncryptedEmailAsync(string encryptedEmail)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Email, encryptedEmail);
        return _users.Find(filter).FirstOrDefaultAsync();
    }
}