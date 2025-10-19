using DziennikPlecakowy.Models;
using System.Security.Claims;

namespace DziennikPlecakowy.Interfaces;

// Interfejs serwisu szyfrowania i tokenów
public interface ICypherService
{
    public string Encrypt(string text);
    public string Decrypt(string text);
    public string GenerateJwtToken(User user);
    public ClaimsPrincipal ValidateJwtToken(string token);
    public Task<User?> GetUserInfoFromTokenAsync(string token);
    public string GetUserIdFromToken(string token);
}
