using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DziennikPlecakowy.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;

        public AuthController(IAuthService authService, IUserService userService)
        {
            _authService = authService;
            _userService = userService;
        }

        // --- AUTH ---
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequest userRegisterData)
        {
            try
            {
                var user = await _userService.GetUserByEmail(userRegisterData.Email);
                if (user != null)
                {
                    return BadRequest("Użytkownik o podanym adresie email już istnieje.");
                }

                var result = await _userService.UserRegister(userRegisterData);
                return result > 0 ? Ok(result) : BadRequest("Nie udało się zarejestrować użytkownika.");
            }
            catch (Exception e)
            {
                return BadRequest("Błąd podczas rejestracji: " + e.Message);
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserAuthRequest userAuthData)
        {
            try
            {
                var authData = await _authService.Login(userAuthData);
                return authData != null ? Ok(authData) : Unauthorized("Nieprawidłowe dane logowania.");
            }
            catch (Exception e)
            {
                return Unauthorized("Błąd logowania: " + e.Message);
            }
        }
    }
}
