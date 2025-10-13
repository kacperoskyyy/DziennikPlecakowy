using DziennikPlecakowy.Models;
using System.Threading.Tasks;

namespace DziennikPlecakowy.Interfaces
{
    // Interfejs repozytorium tokenów odświeżających
    public interface IRefreshTokenRepository
    {
        Task AddAsync(RefreshToken refreshToken);
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task DeleteAsync(string tokenId);
    }
}