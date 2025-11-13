using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace DziennikPlecakowy.API.Controllers;

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
    public async Task<IActionResult> Register([FromBody] UserRegisterRequestDTO request)
    {
        _logger.LogInformation("Endpoint: POST api/Auth/register invoked for email {Email}.", request.Email);

        try
        {
            await _authService.RegisterAsync(request); 

            return Ok(new { Message = "Rejestracja zakończona pomyślnie." });
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            _logger.LogWarning(ex, "Duplicate key error during registration for {Email}.", request.Email);
            return Conflict("Użytkownik o tym adresie e-mail już istnieje.");
        }
        catch (System.Exception e)
        {
            _logger.LogError(e, "Unexpected error during registration for email {Email}.", request.Email);
            return StatusCode(500, "Wystąpił nieoczekiwany błąd serwera.");
        }
    }
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

    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDTO request)
    {
        _logger.LogInformation("Endpoint: POST api/Auth/logout invoked.");

        if (request == null || string.IsNullOrEmpty(request.RefreshToken))
        {
            return Ok(new { Message = "Wylogowano." });
        }

        try
        {
            await _authService.LogoutAsync(request.RefreshToken);
            return Ok(new { Message = "Wylogowano pomyślnie." });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error during remote logout.");
            return Ok(new { Message = "Wylogowano (z błędem serwera)." });
        }
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO request)
    {
        _logger.LogInformation("Endpoint: POST api/Auth/forgot-password invoked for email {Email}.", request.Email);

        try
        {
            await _authService.RequestPasswordResetAsync(request.Email);

            return Ok(new { Message = "Jeśli konto istnieje, wysłano instrukcje resetowania." });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error during forgot-password for email {Email}.", request.Email);
            return Ok(new { Message = "Jeśli konto istnieje, wysłano instrukcje resetowania." });
        }
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDTO request)
    {
        _logger.LogInformation("Endpoint: POST api/Auth/reset-password invoked.");

        if (string.IsNullOrEmpty(request.Token) || string.IsNullOrEmpty(request.NewPassword))
        {
            return BadRequest(new ErrorResponseDTO { Message = "Token i nowe hasło są wymagane." });
        }

        try
        {
            var success = await _authService.ResetPasswordAsync(request.Token, request.NewPassword);

            if (success)
            {
                _logger.LogInformation("Password reset successful for user associated with token.");
                return Ok(new { Message = "Hasło zostało pomyślnie zmienione." });
            }

            _logger.LogWarning("Password reset failed for token (first 3 chars): {TokenStart}", request.Token.Substring(0, Math.Min(3, request.Token.Length)));
            return BadRequest(new ErrorResponseDTO { Message = "Nieprawidłowy lub wygasły token, lub hasło było już używane." });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error during reset-password.");
            return StatusCode(500, new ErrorResponseDTO { Message = "Wystąpił nieoczekiwany błąd serwera." });
        }
    }
}