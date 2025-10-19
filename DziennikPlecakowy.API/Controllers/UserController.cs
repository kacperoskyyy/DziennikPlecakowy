using Microsoft.AspNetCore.Mvc;
using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace DziennikPlecakowy.API.Controllers;

//Kontroler zarządzania użytkownikami
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "User, Admin")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;
    // Konstruktor kontrolera ze wstrzykiwaniem zależności
    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }
    // Pobierz Id użytkownika z tokena JWT
    private string? GetUserIdFromToken()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
    // Endpoint zmiany nazwy użytkownika
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
    // Endpoint zmiany hasła użytkownika
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
    // Endpoint zmiany emaila użytkownika
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
    // Endpoint usunięcia konta użytkownika
    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteUser()
    {
        _logger.LogInformation("Endpoint: DELETE api/User/delete invoked.");

        var userId = GetUserIdFromToken();
        if (userId == null) return Unauthorized();

        try
        {
            var success = await _userService.DeleteUserAsync(userId);

            if (success)
            {
                _logger.LogInformation("User {UserId} deleted successfully.", userId);
                return Ok(new { Message = "Użytkownik został pomyślnie usunięty." });
            }

            _logger.LogWarning("Failed to delete user {UserId}.", userId);
            return NotFound("Nie udało się usunąć użytkownika. Nie znaleziono użytkownika.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during delete user {UserId}.", userId);
            return StatusCode(500, "Wystąpił nieoczekiwany błąd serwera.");
        }
    }
    // Endpoint ustawienia daty ostatniego logowania
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
    // Endpoint nadania roli Admin innemu użytkownikowi (tylko dla Adminów)
    [HttpPut("makeAdmin/{targetUserId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> MakeAdminAsync(string targetUserId)
    {
        _logger.LogInformation("Endpoint: PUT api/User/makeAdmin invoked by Admin {AdminId} for user {TargetId}.", GetUserIdFromToken(), targetUserId);

        try
        {
            var user = await _userService.GetUserById(targetUserId);
            if (user == null)
            {
                _logger.LogWarning("MakeAdmin failed: Target user {TargetId} not found.", targetUserId);
                return NotFound("Nie znaleziono użytkownika docelowego.");
            }

            var result = await _userService.SetAdmin(user);

            if (result > 0)
            {
                _logger.LogInformation("User {TargetId} successfully granted Admin role.", targetUserId);
                return Ok(new { Message = "Rola administratora pomyślnie nadana." });
            }

            _logger.LogWarning("MakeAdmin failed for user {TargetId}.", targetUserId);
            return BadRequest("Nie udało się potwierdzić roli Admin.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during makeAdmin for user {TargetId}.", targetUserId);
            return StatusCode(500, "Wystąpił nieoczekiwany błąd serwera.");
        }
    }
    // Endpoint sprawdzenia, czy dany użytkownik ma rolę Admin (tylko dla Adminów)
    [HttpGet("checkAdmin/{targetUserId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CheckAdminAsync(string targetUserId)
    {
        _logger.LogInformation("Endpoint: GET api/User/checkAdmin invoked by Admin {AdminId} for user {TargetId}.", GetUserIdFromToken(), targetUserId);

        try
        {
            var user = await _userService.GetUserById(targetUserId);
            if (user == null) return NotFound("Nie znaleziono użytkownika.");

            var isAdmin = user.Roles.Contains(UserRole.Admin);

            if (isAdmin)
            {
                _logger.LogInformation("User {TargetId} is an Admin.", targetUserId);
                return Ok(new { IsAdmin = true, Message = "Użytkownik ma rolę Admina." });
            }

            _logger.LogInformation("User {TargetId} is NOT an Admin.", targetUserId);
            return Ok(new { IsAdmin = false, Message = "Użytkownik nie ma roli Admina." });

        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during checkAdmin for user {TargetId}.", targetUserId);
            return StatusCode(500, "Wystąpił nieoczekiwany błąd serwera.");
        }
    }
}