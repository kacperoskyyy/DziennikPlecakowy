using Microsoft.AspNetCore.Mvc;
using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace DziennikPlecakowy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "User, Admin")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAuthService _authService;
    private readonly ILogger<UserController> _logger;
    public UserController(IUserService userService, ILogger<UserController> logger, IAuthService authService)
    {
        _userService = userService;
        _logger = logger;
        _authService = authService;
    }
    private string? GetUserIdFromToken()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
    [HttpPut("changeName")]
    public async Task<IActionResult> ChangeName([FromBody] UserChangeNameRequestDTO request)
    {
        _logger.LogInformation("Endpoint: PUT api/User/changeName invoked.");

        var userId = GetUserIdFromToken();
        if (userId == null) return Unauthorized();

        try
        {
            var success = await _userService.ChangeUsernameAsync(userId, request.NewUsername);

            if (success)
            {
                _logger.LogInformation("Username updated successfully for user {UserId}.", userId);
                return Ok(new { Message = "Nazwa użytkownika została pomyślnie zmieniona." });
            }

            _logger.LogWarning("Failed to update username for user {UserId}.", userId);
            return BadRequest("Nie udało się zaktualizować nazwy użytkownika. Użytkownik nie istnieje lub nazwa jest zajęta.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during changeName for user {UserId}.", userId);
            return StatusCode(500, "Wystąpił nieoczekiwany błąd serwera.");
        }
    }
    [HttpPut("changePassword")]
    public async Task<IActionResult> ChangePassword([FromBody] UserChangePasswordRequestDTO request)
    {
        _logger.LogInformation("Endpoint: PUT api/User/changePassword invoked.");

        var userId = GetUserIdFromToken();
        if (userId == null) return Unauthorized();

        try
        {
            var success = await _userService.ChangePasswordAsync(userId, request.Password, request.NewPassword);

            if (success)
            {
                _logger.LogInformation("Password changed successfully for user {UserId}.", userId);
                return Ok(new { Message = "Hasło zostało pomyślnie zmienione." });
            }

            _logger.LogWarning("Failed to change password for user {UserId}.", userId);
            return Unauthorized("Nieprawidłowe hasło lub nie udało się zmienić hasła.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during changePassword for user {UserId}.", userId);
            return StatusCode(500, "Wystąpił nieoczekiwany błąd serwera.");
        }
    }
    [HttpPut("changeEmail")]
    public async Task<IActionResult> ChangeEmail([FromBody] UserChangeEmailRequestDTO request)
    {
        _logger.LogInformation("Endpoint: PUT api/User/changeEmail invoked.");

        var userId = GetUserIdFromToken();
        if (userId == null) return Unauthorized();

        try
        {
            var success = await _userService.ChangeEmailAsync(userId, request.NewEmail);

            if (success)
            {
                _logger.LogInformation("Email updated successfully for user {UserId}.", userId);
                return Ok(new { Message = "Email został pomyślnie zmieniony." });
            }

            _logger.LogWarning("Failed to update email for user {UserId}.", userId);
            return BadRequest("Nie udało się zaktualizować emaila. Użytkownik nie istnieje lub email jest zajęty.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during changeEmail for user {UserId}.", userId);
            return StatusCode(500, "Wystąpił nieoczekiwany błąd serwera.");
        }
    }
    [HttpPost("request-deletion")]
    public async Task<IActionResult> RequestAccountDeletion()
    {
        _logger.LogInformation("Endpoint: POST api/User/request-deletion invoked.");
        var userId = GetUserIdFromToken();
        if (userId == null) return Unauthorized();

        try
        {
            await _authService.RequestAccountDeletionAsync(userId);
            _logger.LogInformation("Account deletion code sent for user {UserId}.", userId);
            return Ok(new { Message = "Kod potwierdzający został wysłany na Twój adres e-mail." });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during request-deletion for user {UserId}.", userId);
            return StatusCode(500, new ErrorResponseDTO { Message = "Wystąpił błąd podczas wysyłania kodu." });
        }
    }

    [HttpPost("confirm-deletion")]
    public async Task<IActionResult> ConfirmAccountDeletion([FromBody] ConfirmDeletionRequestDTO request)
    {
        _logger.LogInformation("Endpoint: POST api/User/confirm-deletion invoked.");
        var userId = GetUserIdFromToken();
        if (userId == null) return Unauthorized();

        if (string.IsNullOrEmpty(request.Token))
        {
            return BadRequest(new ErrorResponseDTO { Message = "Kod jest wymagany." });
        }

        try
        {
            var success = await _authService.ConfirmAccountDeletionAsync(userId, request.Token);

            if (success)
            {
                _logger.LogInformation("User {UserId} deleted successfully after code confirmation.", userId);
                return Ok(new { Message = "Konto zostało pomyślnie usunięte." });
            }

            _logger.LogWarning("Failed to confirm account deletion for user {UserId}. Invalid token.", userId);
            return BadRequest(new ErrorResponseDTO { Message = "Nieprawidłowy lub wygasły kod." });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during confirm-deletion for user {UserId}.", userId);
            return StatusCode(500, new ErrorResponseDTO { Message = "Wystąpił nieoczekiwany błąd serwera." });
        }
    }

    [HttpGet("setLastLogin")]
    public async Task<IActionResult> SetLastLogin()
    {
        _logger.LogInformation("Endpoint: GET api/User/setLastLogin invoked.");

        var userId = GetUserIdFromToken();
        if (userId == null) return Unauthorized();

        try
        {
            var result = await _userService.SetLastLogin(userId);

            if (result > 0)
            {
                _logger.LogInformation("Last login date updated successfully for user {UserId}.", userId);
                return Ok(new { Message = "Data ostatniego logowania zaktualizowana." });
            }

            _logger.LogWarning("Failed to update last login date for user {UserId}.", userId);
            return NotFound("Nie udało się zaktualizować daty ostatniego logowania.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during setLastLogin for user {UserId}.", userId);
            return StatusCode(500, "Wystąpił nieoczekiwany błąd serwera.");
        }
    }

    [HttpGet("getUserStats")]
    [Authorize(Roles = "User, Admin")]
    public async Task<IActionResult> GetUserStats()
    {
        _logger.LogInformation("Endpoint: GET api/User/getUserStats invoked.");
        var userId = GetUserIdFromToken();
        if (userId == null) return Unauthorized();
        try
        {
            var user = await _userService.GetUserById(userId);
            if (user == null)
            {
                return NotFound("Nie znaleziono użytkownika.");
            }

            var userStats = await _userService.GetUserStats(userId);

            var userProfileDto = new UserProfileDTO
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                Roles = user.Roles.ToList(),
                Stats = new UserStatDTO
                {
                    TripsCount = userStats.TripsCount,
                    TotalDistance = userStats.TotalDistance,
                    TotalDuration = userStats.TotalDuration,
                    TotalElevationGain = userStats.TotalElevationGain,
                    TotalSteps = userStats.TotalSteps
                }
            };

            _logger.LogInformation("User stats retrieved successfully for user {UserId}.", userId);

            return Ok(userProfileDto);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during getUserStats for user {UserId}.", userId);
            return StatusCode(500, "Wystąpił nieoczekiwany błąd serwera.");
        }
    }

}