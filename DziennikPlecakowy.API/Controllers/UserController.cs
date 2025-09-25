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
        public async Task<IActionResult> ChangeName([FromBody] UserChangeNameRequest userChangeName)
        {
            Console.WriteLine("User/changeName");
            try
            {
                var userId = GetUserIdFromToken();
                if (string.IsNullOrEmpty(userId))
                {
                    Console.WriteLine("No Id in user token");
                    return Unauthorized("Brak Id użytkownika w tokenie.");
                }
                    

                var user = await _userService.GetUserById(userId);
                if (user == null) 
                { 
                    Console.WriteLine("User not found");
                    return NotFound("Nie znaleziono użytkownika."); 
                }

                user.Username = userChangeName.NewUsername;
                var result = await _userService.UpdateUser(user);
                if(result > 0)
                    Console.WriteLine("Username updated successfully");
                else
                    Console.WriteLine("Failed to update username");
                return result > 0 ? Ok(result) : BadRequest("Nie udało się zaktualizować nazwy użytkownika.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
                return BadRequest("Błąd: " + e.Message);
            }
        }

        [Authorize(Roles = "User, Admin")]
        [HttpPut("changePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] UserChangePasswordRequest userChangePassword)
        {
            Console.WriteLine("User/changePassword");
            try
            {
                var userId = GetUserIdFromToken();
                if (string.IsNullOrEmpty(userId))
                {
                    Console.WriteLine("No Id in user token");
                    return Unauthorized("Brak Id użytkownika w tokenie.");
                }

                var user = await _userService.GetUserById(userId);
                if (user == null) 
                { 
                    Console.WriteLine("User not found");
                    return NotFound("Nie znaleziono użytkownika."); 
                }

                if (!_userService.CheckPassword(user, userChangePassword.Password))
                {
                    Console.WriteLine("Invalid current password");
                    return Unauthorized("Nieprawidłowe hasło.");
                }
                    

                var result = await _userService.ChangePassword(user, userChangePassword.NewPassword);
                if(result > 0)
                    Console.WriteLine("Password changed successfully");
                else
                    Console.WriteLine("Failed to change password");
                return result > 0 ? Ok(result) : BadRequest("Nie udało się zmienić hasła.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
                return BadRequest("Błąd: " + e.Message);
            }
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPut("changeEmail")]
        public async Task<IActionResult> ChangeEmail([FromBody] UserChangeEmailRequest userChangeEmail)
        {
            Console.WriteLine("User/changeEmail");
            try
            {
                var userId = GetUserIdFromToken();
                if (string.IsNullOrEmpty(userId))
                {
                    Console.WriteLine("No Id in user token");
                    return Unauthorized("Brak Id użytkownika w tokenie.");
                }
                   

                var user = await _userService.GetUserById(userId);
                if (user == null) 
                { 
                    Console.WriteLine("User not found");
                    return NotFound("Nie znaleziono użytkownika."); 
                }

                var result = await _userService.ChangeEmail(user, userChangeEmail.NewEmail);
                if(result > 0)
                    Console.WriteLine("Email updated successfully");
                else
                    Console.WriteLine("Failed to update email");
                return result > 0 ? Ok(result) : BadRequest("Nie udało się zaktualizować emaila.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
                return BadRequest("Błąd: " + e.Message);
            }
        }

        [Authorize(Roles = "User, Admin")]
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteUser()
        {
            Console.WriteLine("User/delete");
            try
            {
                var userId = GetUserIdFromToken();
                if (string.IsNullOrEmpty(userId))
                {
                    Console.WriteLine("No Id in user token");
                    return Unauthorized("Brak Id użytkownika w tokenie.");
                }

                var user = await _userService.GetUserById(userId);
                if (user == null)
                {
                    Console.WriteLine("User not found");
                    return NotFound("Nie znaleziono użytkownika.");
                }

                var result = await _userService.DeleteUser(user);
                if(result > 0)
                    Console.WriteLine("User deleted successfully");
                else
                    Console.WriteLine("Failed to delete user");
                return result > 0 ? Ok(result) : BadRequest("Nie udało się usunąć użytkownika.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
                return BadRequest("Błąd: " + e.Message);
            }
        }

        [Authorize(Roles = "User, Admin")]
        [HttpPost("setLastLogin")]
        public async Task<IActionResult> SetLastLogin()
        {
            Console.WriteLine("User/setLastLogin");
            try
            {
                var userId = GetUserIdFromToken();
                if (string.IsNullOrEmpty(userId))
                {
                    Console.WriteLine("No Id in user token");
                    return Unauthorized("Brak Id użytkownika w tokenie.");
                }
                    

                var result = await _userService.SetLastLogin(userId);
                if(result > 0)
                    Console.WriteLine("Last login date updated successfully");
                else
                    Console.WriteLine("Failed to update last login date");
                return result > 0 ? Ok(result) : BadRequest("Nie udało się zaktualizować daty ostatniego logowania.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
                return BadRequest("Błąd: " + e.Message);
            }
        }

        // --- ROLE MANAGEMENT ---
        [Authorize(Roles = "Admin")]
        [HttpPut("makeAdmin")]

        public async Task<IActionResult> MakeAdminAsync(User user)
        {
            Console.WriteLine("User/makeAdmin");
            if (user == null)
            {
                Console.WriteLine("No user in token");
                return Unauthorized("Brak użytkownika w tokenie.");
            }
            if (!user.Roles.Contains(UserRole.Admin))
            {
                Console.WriteLine("User is not Admin");
                return Unauthorized("Użytkownik nie jest Adminem.");
            }
            var result = await _userService.SetAdmin(user);
            return result > 0 ? Ok(true) : BadRequest("Nie udało się potwierdzić roli Admin.");
        }
      

        // --- SIMPLE ROLE CHECKS ---
        [Authorize(Roles = "Admin")]
        [HttpGet("checkAdmin")]
        public async Task<IActionResult> CheckAdminAsync([FromBody] string Id)
        {
            Console.WriteLine("User/checkAdmin");
            try
            {
                var user = await _userService.GetUserById(Id);
                if (user == null)
                {
                    Console.WriteLine("User not found");
                    return NotFound("Nie znaleziono użytkownika.");
                }

                var result = !user.Roles.Contains(UserRole.Admin);
                if (result)
                    Console.WriteLine("User is not an Admin");
                else
                    Console.WriteLine("User is Admin");
                return result ? Ok(result) : BadRequest(result);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
                return Unauthorized("Błąd: " + e.Message);
            }
        }

    }
}
