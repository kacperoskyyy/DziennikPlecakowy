using Microsoft.AspNetCore.Mvc;
using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace DziennikPlecakowy.API.Controllers
{
    //Kontroler logowania i rejestracji
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        // Konstruktor kontrolera ze wstrzykiwaniem zależności
        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }
        // Endpoint rejestracji nowego użytkownika
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequest request)
        {
            _logger.LogInformation("Endpoint: POST api/Auth/register invoked for email {Email}.", request.Email);

            try
            {
                var success = await _authService.RegisterAsync(request);

                if (success)
                {
                    return Ok(new { Message = "Rejestracja zakończona pomyślnie." });
                }

                return BadRequest("Rejestracja nie powiodła się. Sprawdź, czy email nie jest już zajęty.");
            }
            catch (System.Exception e)
            {
                _logger.LogError(e, "Unexpected error during registration for email {Email}.", request.Email);
                return StatusCode(500, "Wystąpił nieoczekiwany błąd serwera.");
            }
        }
        // Endpoint logowania użytkownika
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserAuthRequest userAuthData)
        {
            _logger.LogInformation("Endpoint: POST api/Auth/login invoked for email {Email}.", userAuthData.Email);

            try
            {
                var authResponse = await _authService.Login(userAuthData);

                if (authResponse != null)
                {
                    _logger.LogInformation("User logged in successfully with email {Email}.", userAuthData.Email);
                    return Ok(new
                    {
                        Token = authResponse.Token,
                        RefreshToken = authResponse.RefreshToken,
                        Message = "Pomyślnie zalogowano."
                    });
                }

                return Unauthorized("Nieprawidłowy login lub hasło.");
            }
            catch (System.Exception e)
            {
                _logger.LogError(e, "Unexpected error during login for email {Email}.", userAuthData.Email);
                return StatusCode(500, "Wystąpił nieoczekiwany błąd serwera.");
            }
        }

        // Endpoint odświeżania tokenu
        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            _logger.LogInformation("Endpoint: POST api/Auth/refresh invoked.");

            if (request == null || string.IsNullOrEmpty(request.RefreshToken))
            {
                return BadRequest("Refresh token jest wymagany.");
            }

            try
            {
                var authResponse = await _authService.RefreshTokenAsync(request.RefreshToken);

                if (authResponse != null)
                {
                    return Ok(new
                    {
                        Token = authResponse.Token,
                        RefreshToken = authResponse.RefreshToken
                    });
                }

                return Unauthorized("Nieprawidłowy lub wygasły refresh token.");
            }
            catch (System.Exception e)
            {
                _logger.LogError(e, "Unexpected error during token refresh.");
                return StatusCode(500, "Wystąpił nieoczekiwany błąd serwera.");
            }
        }
    }
}