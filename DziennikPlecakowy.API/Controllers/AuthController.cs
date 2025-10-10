using Microsoft.AspNetCore.Mvc;
using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace DziennikPlecakowy.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

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
                    _logger.LogInformation("User registration successful for email {Email}.", request.Email);
                    return Ok(new { Message = "Rejestracja zakończona pomyślnie." });
                }

                _logger.LogWarning("User registration failed for email {Email}.", request.Email);
                return BadRequest("Rejestracja nie powiodła się. Sprawdź, czy email nie jest już zajęty.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error during registration for email {Email}.", request.Email);
                return StatusCode(500, "Wystąpił nieoczekiwany błąd serwera.");
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserAuthRequest userAuthData)
        {
            _logger.LogInformation("Endpoint: POST api/Auth/login invoked for email {Email}.", userAuthData.Email);

            try
            {
                var token = await _authService.Login(userAuthData);

                if (token != null)
                {
                    _logger.LogInformation("User logged in successfully with email {Email}.", userAuthData.Email);
                    return Ok(new { Token = token, Message = "Pomyślnie zalogowano." });
                }

                _logger.LogWarning("Login failed for email {Email}. Invalid credentials.", userAuthData.Email);
                return Unauthorized("Nieprawidłowy login lub hasło.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error during login for email {Email}.", userAuthData.Email);
                return StatusCode(500, "Wystąpił nieoczekiwany błąd serwera.");
            }
        }
    }
}