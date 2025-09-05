using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DziennikPlecakowy.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly ICypherService _cypherService;

        public AuthService(IUserService userService,  IConfiguration configuration, ICypherService cypherService )
        {
            _userService = userService;
            _configuration = configuration;
            _cypherService = cypherService;
        }

        public async Task<string> Login(UserAuthRequest userAuthData)
        {
            try
            {
                User? user = await _userService.GetUserByEmail(userAuthData.Email);
                if (user == null || !_userService.CheckPassword(user, userAuthData.Password))
                {
                    return null;
                }
                var token = _cypherService.GenerateJwtToken(user.Username);

                await _userService.SetLastLogin(user.Id);
                return token;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
                return null;
            }
        }
        //TU JEST PROBLEM
        public async Task<User> GetUserInfoFromToken(string token)
        {
            try
            {
                var claims = _cypherService.ValidateJwtToken(token);
                var userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return null;
                }
                return await _userService.GetUserById(userId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
                return null;
            }
        }


    }
}
