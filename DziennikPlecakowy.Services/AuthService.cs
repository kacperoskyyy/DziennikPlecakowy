using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.Models;

namespace DziennikPlecakowy.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserService _userService;
        //Konstruktor
        public AuthService(IUserService userService)
        {
            _userService = userService;
        }
        //Metoda służaca do logowania z obsługą błędów
        public async Task<AuthData> Login(UserAuthRequest userAuthData)
        {
            try
            {
                //Pobranie użytkownika po emailu
                User? user = await _userService.GetUserByEmail(userAuthData.Email);
                if (user == null)
                {
                    return null;
                }
                //Sprawdzenie hasła
                if (_userService.CheckPassword(user,userAuthData.Password))
                {
                    //Ustawienie daty ostatniego logowania i zwrócenie danych
                    await _userService.SetLastLogin(user.Id);
                    return new AuthData(user.Id, user.Username,user.Email,user.IsAdmin, user.IsSuperUser);
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
                return null;
            }
        }
    }
}
