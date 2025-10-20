using Microsoft.AspNetCore.Mvc;
using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace DziennikPlecakowy.API.Controllers;

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
    public async Task<IActionResult> Register([FromBody] UserRegisterRequestDTO request)
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
    public async Task<IActionResult> Login([FromBody] UserAuthRequestDTO userAuthData)
    {
        _logger.LogInformation("Endpoint: POST api/Auth/login invoked for email {Email}.", userAuthData.Email);

        try
        {
            var authResponse = await _authService.Login(userAuthData);

            if (authResponse != null)
            {
                if(authResponse.MustChangePassword)
                {
                    _logger.LogInformation("User with email {Email} must change password.", userAuthData.Email);
                    return Ok(new
                    {
                        Token = authResponse.Token,
                        RefreshToken = authResponse.RefreshToken,
                        MustChangePassword = true,
                        Message = "Wymagana jest zmiana hasła."
                    });
                }

                if(authResponse.Token == "LOCKED" && authResponse.RefreshToken == "LOCKED")
                {
                    _logger.LogWarning("Login attempt for email {Email}, but user blocked", userAuthData.Email);
                    return Unauthorized("Uzytkownik zablokowany.");
                }

                _logger.LogInformation("User logged in successfully with email {Email}.", userAuthData.Email);
                return Ok(new
                {
                    Token = authResponse.Token,
                    RefreshToken = authResponse.RefreshToken,
                    MustChangePassword = false,
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
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDTO request)
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