using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DziennikPlecakowy.Models;


namespace DziennikPlecakowy.Services
{
    public class CypherService : ICypherService
    {
        private static string? _key;
        private static string? _iv;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        public CypherService(IConfiguration _config, IUserService userService)
        {
            _key = _config["Cypher:Key"];
            _iv = _config["Cypher:IV"];
            _configuration = _config;
            _userService = userService;
        }
        //Metoda do zaszyfrowania tekstu
        public string Encrypt(string text)
        {
            byte[] textBytes = Encoding.UTF8.GetBytes(text);
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(_key);
                aesAlg.IV = Encoding.UTF8.GetBytes(_iv);
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(textBytes, 0, textBytes.Length);
                        csEncrypt.Close();
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }
        //Metoda do odszyfrowania tekstu
        public string Decrypt(string text)
        {
            byte[] textBytes = Convert.FromBase64String(text);
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(_key);
                aesAlg.IV = Encoding.UTF8.GetBytes(_iv);
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msDecrypt = new MemoryStream(textBytes))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
        //Generowanie tokena
        public string GenerateJwtToken(string Id)
        {
            var claims = new[]
            {
                    new Claim(ClaimTypes.NameIdentifier, Id),
                };

            // Pobranie klucza i konfiguracji z appsettings.json
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        //Walidacja tokena
        public ClaimsPrincipal ValidateJwtToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key), "Jwt:Key cannot be null or empty.");
            }

            var keyBytes = Encoding.UTF8.GetBytes(key);
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal;
            }
            catch (Exception ex)
            {
                throw new SecurityTokenException("Invalid token.", ex);
            }
        }
        public async Task<User> GetUserInfoFromTokenAsync(string token)
        {
            var principal = ValidateJwtToken(token);
            var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                throw new SecurityTokenException("Token does not contain a valid user ID.");
            }

            var userId = userIdClaim.Value;
            var user = await _userService.GetUserById(userId);

            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }

            return new User
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Roles = user.Roles
            };
        }

    }
}
