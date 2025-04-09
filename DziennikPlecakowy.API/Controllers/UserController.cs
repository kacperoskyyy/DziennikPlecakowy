using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DziennikPlecakowy.DTO;
using DziennikPlecakowy.Interfaces;
using DziennikPlecakowy.Models;
using Microsoft.AspNetCore.Authorization;

namespace DziennikPlecakowy.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;
        public UserController(IUserService userService,IAuthService authService)
        {
            _userService = userService;
            _authService = authService;
        }
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
                if (result > 0)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest("Nie udało się zarejestrować użytkownika.");
                }
            }
            catch (Exception e)
            {
                return BadRequest("Nie udało się zarejestrować użytkownika. " + e);
            }
        }
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserAuthRequest userAuthData)
        {
            try
            {
                var authData = await _authService.Login(userAuthData);
                if (authData != null)
                {
                    return Ok(authData);
                }
                else
                {
                    return Unauthorized("Nieprawidłowe dane logowania.");
                }
            }
            catch (Exception e)
            {
                return Unauthorized("Nieprawidłowe dane logowania. " + e);
            }
        }
        [Authorize(Roles = "User, SuperUser, Admin")]
        [HttpPut("changeName")]
        public async Task<IActionResult> UpdateName([FromBody] UserChangeNameRequest userChangeName)
        {
            try
            {
                var user = await _userService.GetUserById(userChangeName.UserId);
                if (user == null)
                {
                    return NotFound("Nie znaleziono użytkownika o podanym Id.");
                }
                user.Username = userChangeName.NewUsername;
                var result = await _userService.UpdateUser(user);
                if (result > 0)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest("Nie udało się zaktualizować danych użytkownika.");
                }
            }
            catch (Exception e)
            {
                return BadRequest("Nie udało się zaktualizować danych użytkownika. " + e);
            }
        }
        [Authorize(Roles = "User, SuperUser, Admin")]
        [HttpPut("changePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] UserChangePasswordRequest userChangePassword)
        {
            try
            {
                var user = await _userService.GetUserById(userChangePassword.UserId);
                if (user == null)
                {
                    return NotFound("Nie znaleziono użytkownika o podanym Id.");
                }
                if (!_userService.CheckPassword(user, userChangePassword.Password))
                {
                    return Unauthorized("Nieprawidłowe hasło.");
                }
                var result = await _userService.ChangePassword(user, userChangePassword.NewPassword);
                if (result > 0)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest("Nie udało się zaktualizować hasła.");
                }
            }
            catch (Exception e)
            {
                return BadRequest("Nie udało się zaktualizować hasła. " + e);
            }
        }
        [Authorize(Roles = "User, SuperUser, Admin")]
        [HttpPut("changeEmail")]
        public async Task<IActionResult> ChangeEmail([FromBody] UserChangeEmailRequest userChangeEmail)
        {
            try
            {
                var user = await _userService.GetUserById(userChangeEmail.UserId);
                if (user == null)
                {
                    return NotFound("Nie znaleziono użytkownika o podanym Id.");
                }
                var result = await _userService.ChangeEmail(user, userChangeEmail.NewEmail);
                if (result > 0)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest("Nie udało się zaktualizować adresu email.");
                }
            }
            catch (Exception e)
            {
                return BadRequest("Nie udało się zaktualizować adresu email. " + e);
            }
        }
        [Authorize(Roles = "User, SuperUser, Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                var user = await _userService.GetUserById(id);
                if (user == null)
                {
                    return NotFound("Nie znaleziono użytkownika o podanym Id.");
                }
                var result = await _userService.DeleteUser(user);
                if (result > 0)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest("Nie udało się usunąć użytkownika.");
                }
            }
            catch (Exception e)
            {
                return BadRequest("Nie udało się usunąć użytkownika. " + e);
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("makeAdmin/{Id}")]
        public async Task<IActionResult> ChangeRole([FromBody] string Id)
        {
            try
            {
                var user = await _userService.GetUserById(Id);
                if (user == null)
                {
                    return NotFound("Nie znaleziono użytkownika o podanym Id.");
                }
                user.Roles.Add(UserRole.Admin);
                var result = await _userService.UpdateUser(user);
                if (result > 0)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest("Nie udało się zaktualizować roli użytkownika.");
                }
            }
            catch (Exception e)
            {
                return Unauthorized("Nie masz uprawnień do wykonania tej operacji. " + e);
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("checkAdmin/{Id}")]
        public async Task<IActionResult> CheckAdmin([FromBody]string Id)
        {
            try
            {
                var result = _userService.IsAdmin(Id);
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest("Nie udało się sprawdzić uprawnień użytkownika. " + e);
            }

        }
        [Authorize(Roles = "User, SuperUser, Admin")]
        [HttpPost("setLastLogin/{Id}")]
        public async Task<IActionResult> SetLastLogin([FromBody]string Id)
        {
            try
            {
                var result = await _userService.SetLastLogin(Id);
                if (result > 0)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest("Nie udało się zaktualizować daty ostatniego logowania.");
                }
            }
            catch (Exception e)
            {
                return BadRequest("Nie udało się zaktualizować daty ostatniego logowania. " + e);
            }
        }
        [Authorize]
        [HttpGet("checkSuperUser/{Id}")]
        public async Task<IActionResult> ActionResult([FromBody] string Id)
        {
            try
            {
                var user = await _userService.GetUserById(Id);
                if (user == null)
                {
                    return NotFound("Nie znaleziono użytkownika o podanym Id.");
                }
                if (await _userService.CheckIsSuperUser(user))
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("Nie udało się sprawdzić uprawnień superużytkownika.");
                }
            }
            catch (Exception e)
            {
                return BadRequest("Nie udało się sprawdzić uprawnień superużytkownika. " + e);
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("setSuperUser/{Id}")]
        public async Task<IActionResult> SetSuperUser([FromBody] string Id)
        {
            try
            {
                var user = await _userService.GetUserById(Id);
                if (user == null)
                {
                    return NotFound("Nie znaleziono użytkownika o podanym Id.");
                }
                user.Roles.Add(UserRole.SuperUser);
                var result = await _userService.UpdateUser(user);
                if (result > 0)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest("Nie udało się zaktualizować roli superużytkownika.");
                }
            }
            catch (Exception e)
            {
                return Unauthorized("Nie masz uprawnień do wykonania tej operacji. " + e);
            }
        }
    }
}
