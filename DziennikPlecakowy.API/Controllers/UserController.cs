using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace DziennikPlecakowy.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;

        public UserController(IUserService userService, IAuthService authService)
        {
            _userService = userService;
            _authService = authService;
        }

        private string? GetUserIdFromToken()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        // --- USER ACTIONS ---
        [Authorize(Roles = "User, Admin")]
        [HttpPut("changeName")]
        public async Task<IActionResult> UpdateName([FromBody] UserChangeNameRequest userChangeName)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("Brak Id użytkownika w tokenie.");

                var user = await _userService.GetUserById(userId);
                if (user == null) return NotFound("Nie znaleziono użytkownika.");

                user.Username = userChangeName.NewUsername;
                var result = await _userService.UpdateUser(user);
                return result > 0 ? Ok(result) : BadRequest("Nie udało się zaktualizować nazwy użytkownika.");
            }
            catch (Exception e)
            {
                return BadRequest("Błąd: " + e.Message);
            }
        }

        [Authorize(Roles = "User, Admin")]
        [HttpPut("changePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] UserChangePasswordRequest userChangePassword)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("Brak Id użytkownika w tokenie.");

                var user = await _userService.GetUserById(userId);
                if (user == null) return NotFound("Nie znaleziono użytkownika.");

                if (!_userService.CheckPassword(user, userChangePassword.Password))
                    return Unauthorized("Nieprawidłowe hasło.");

                var result = await _userService.ChangePassword(user, userChangePassword.NewPassword);
                return result > 0 ? Ok(result) : BadRequest("Nie udało się zmienić hasła.");
            }
            catch (Exception e)
            {
                return BadRequest("Błąd: " + e.Message);
            }
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPut("changeEmail")]
        public async Task<IActionResult> ChangeEmail([FromBody] UserChangeEmailRequest userChangeEmail)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("Brak Id użytkownika w tokenie.");

                var user = await _userService.GetUserById(userId);
                if (user == null) return NotFound("Nie znaleziono użytkownika.");

                var result = await _userService.ChangeEmail(user, userChangeEmail.NewEmail);
                return result > 0 ? Ok(result) : BadRequest("Nie udało się zaktualizować emaila.");
            }
            catch (Exception e)
            {
                return BadRequest("Błąd: " + e.Message);
            }
        }

        [Authorize(Roles = "User, Admin")]
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteUser()
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("Brak Id użytkownika w tokenie.");

                var user = await _userService.GetUserById(userId);
                if (user == null) return NotFound("Nie znaleziono użytkownika.");

                var result = await _userService.DeleteUser(user);
                return result > 0 ? Ok(result) : BadRequest("Nie udało się usunąć użytkownika.");
            }
            catch (Exception e)
            {
                return BadRequest("Błąd: " + e.Message);
            }
        }

        [Authorize(Roles = "User, Admin")]
        [HttpPost("setLastLogin")]
        public async Task<IActionResult> SetLastLogin()
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("Brak Id użytkownika w tokenie.");

                var result = await _userService.SetLastLogin(userId);
                return result > 0 ? Ok(result) : BadRequest("Nie udało się zaktualizować daty ostatniego logowania.");
            }
            catch (Exception e)
            {
                return BadRequest("Błąd: " + e.Message);
            }
        }

        // --- ROLE MANAGEMENT ---
        [Authorize(Roles = "Admin")]
        [HttpPut("makeAdmin")]
        public async Task<IActionResult> ChangeRole([FromBody] string Id)
        {
            try
            {
                var user = await _userService.GetUserById(Id);
                if (user == null) return NotFound("Nie znaleziono użytkownika.");

                if (!user.Roles.Contains(UserRole.Admin))
                    user.Roles.Add(UserRole.Admin);

                var result = await _userService.UpdateUser(user);
                return result > 0 ? Ok(result) : BadRequest("Nie udało się ustawić roli Admin.");
            }
            catch (Exception e)
            {
                return Unauthorized("Błąd: " + e.Message);
            }
        }

        // --- SIMPLE ROLE CHECKS ---
        [Authorize(Roles = "Admin")]
        [HttpGet("checkAdmin")]
        public IActionResult CheckAdmin()
        {
            return Ok(true);
        }

    }
}
