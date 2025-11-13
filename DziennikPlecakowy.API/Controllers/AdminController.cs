using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DziennikPlecakowy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")] 
public class AdminController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IUserService userService, ILogger<AdminController> logger)
    {
        _userService = userService;
        _logger = logger;
    }


    [HttpGet("getAllUsers")]
    public async Task<IActionResult> GetAllUsers()
    {
        _logger.LogInformation("Endpoint: GET /api/Admin/getAllUsers invoked.");
        try
        {
            var users = await _userService.GetAllUsersAsync();

            var userDtos = users.Select(user => new AdminUserDetailDTO
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                IsLocked = user.IsBlocked, 
                MustChangePassword = user.MustChangePassword,
                Roles = user.Roles.Select(r => r).ToList()
            });

            return Ok(userDtos);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during getAllUsers.");
            return StatusCode(500, "Błąd serwera.");
        }
    }

    [HttpPut("blockUser/{userId}")]
    public async Task<IActionResult> BlockUser(string userId)
    {
        _logger.LogInformation("Endpoint: PUT /api/Admin/blockUser/{userId} invoked.", userId);
        var success = await _userService.SetUserLockStatusAsync(userId, true);

        if (success) return Ok(new { Message = "Użytkownik zablokowany." });
        return NotFound("Nie znaleziono użytkownika.");
    }

    [HttpPut("unblockUser/{userId}")]
    public async Task<IActionResult> UnblockUser(string userId)
    {
        _logger.LogInformation("Endpoint: PUT /api/Admin/unblockUser/{userId} invoked.", userId);
        var success = await _userService.SetUserLockStatusAsync(userId, false);

        if (success) return Ok(new { Message = "Użytkownik odblokowany." });
        return NotFound("Nie znaleziono użytkownika.");
    }

    [HttpPut("forcePasswordChange/{userId}")]
    public async Task<IActionResult> ForcePasswordChange(string userId)
    {
        _logger.LogInformation("Endpoint: PUT /api/Admin/forcePasswordChange/{userId} invoked.", userId);
        var success = await _userService.SetPasswordChangeStatusAsync(userId, true);

        if (success) return Ok(new { Message = "Wymuszono zmianę hasła." });
        return NotFound("Nie znaleziono użytkownika.");
    }

    [HttpPut("changeRole")]
    public async Task<IActionResult> ChangeRole([FromBody] AdminChangeRoleRequestDTO request)
    {
        _logger.LogInformation("Endpoint: PUT /api/Admin/changeRole invoked for user {UserId}.", request.UserId);
        var success = await _userService.SetUserRoleAsync(request.UserId, request.NewRole);

        if (success) return Ok(new { Message = "Rola zmieniona." });
        return BadRequest("Błąd podczas zmiany roli.");
    }
    [HttpDelete("deleteUser/{userId}")]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        _logger.LogInformation("Endpoint: DELETE /api/Admin/deleteUser/{userId} invoked.", userId);
        try
        {
            var success = await _userService.DeleteUserAsync(userId);

            if (success)
            {
                return Ok(new { Message = "Użytkownik usunięty." });
            }
            return NotFound("Nie znaleziono użytkownika.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during deleteUser for {userId}.", userId);
            return StatusCode(500, "Błąd serwera.");
        }
    }
}