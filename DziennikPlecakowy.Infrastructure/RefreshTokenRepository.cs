using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.Models;
using MongoDB.Driver;

namespace DziennikPlecakowy.Infrastructure
{
    // Implementacja repozytorium dla tokenów odświeżania
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly IMongoCollection<RefreshToken> _refreshTokens;
        // Konstruktor inicjalizujący kolekcję tokenów odświeżania
        public RefreshTokenRepository(IMongoDbContext context)
        {
            _refreshTokens = context.RefreshTokens;
        }
        // Metoda dodająca nowy token odświeżania do bazy danych
        public async Task AddAsync(RefreshToken refreshToken)
        {
            await _refreshTokens.InsertOneAsync(refreshToken);
        }
        // Metoda pobierająca token odświeżania na podstawie wartości tokena
        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _refreshTokens.Find(rt => rt.Token == token).FirstOrDefaultAsync();
        }
        // Metoda usuwająca token odświeżania na podstawie jego identyfikatora
        public async Task DeleteAsync(string tokenId)
        {
            await _refreshTokens.DeleteOneAsync(rt => rt.Id == tokenId);
        }
    }
}