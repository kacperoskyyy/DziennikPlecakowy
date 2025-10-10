using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.Models;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace DziennikPlecakowy.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserService _userService;
        private readonly ICypherService _cypherService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public AuthService(
            IUserService userService,
            ICypherService cypherService,
            IRefreshTokenRepository refreshTokenRepository)
        {
            _userService = userService;
            _cypherService = cypherService;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<bool> RegisterAsync(UserRegisterRequest request)
        {
            var result = await _userService.UserRegister(request);
            return result > 0;
        }

        public async Task<AuthResponse?> Login(UserAuthRequest userAuthData)
        {
            var user = await _userService.GetUserByEmail(userAuthData.Email);

            if (user == null || !_userService.CheckPassword(user, userAuthData.Password))
            {
                return null;
            }

            var jwtToken = _cypherService.GenerateJwtToken(user);
            var refreshToken = await CreateAndStoreRefreshToken(user.Id);

            await _userService.SetLastLogin(user.Id);

            return new AuthResponse
            {
                Token = jwtToken,
                RefreshToken = refreshToken.Token
            };
        }

        public async Task<AuthResponse?> RefreshTokenAsync(string token)
        {
            var storedToken = await _refreshTokenRepository.GetByTokenAsync(token);

            if (storedToken == null || storedToken.ExpiryDate <= DateTime.UtcNow)
            {
                return null;
            }

            var user = await _userService.GetUserById(storedToken.UserId);
            if (user == null)
            {
                return null;
            }

            var newJwtToken = _cypherService.GenerateJwtToken(user);
            var newRefreshToken = await CreateAndStoreRefreshToken(user.Id);

            await _refreshTokenRepository.DeleteAsync(storedToken.Id);

            return new AuthResponse
            {
                Token = newJwtToken,
                RefreshToken = newRefreshToken.Token
            };
        }

        private async Task<RefreshToken> CreateAndStoreRefreshToken(string userId)
        {
            var refreshToken = new RefreshToken
            {
                UserId = userId,
                Token = GenerateRefreshTokenString(),
                ExpiryDate = DateTime.UtcNow.AddMonths(1)
            };

            await _refreshTokenRepository.AddAsync(refreshToken);
            return refreshToken;
        }

        private string GenerateRefreshTokenString()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}