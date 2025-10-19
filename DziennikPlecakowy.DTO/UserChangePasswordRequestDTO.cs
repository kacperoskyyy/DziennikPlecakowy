namespace DziennikPlecakowy.DTO;

// DTO dla żądania zmiany hasła użytkownika
public class UserChangePasswordRequestDTO
{
    public string Password { get; set; }
    public string NewPassword { get; set; }
}
