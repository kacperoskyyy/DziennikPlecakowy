namespace DziennikPlecakowy.DTO;

// DTO dla żądania uwierzytelniania użytkownika
public class UserAuthRequestDTO
{
    public string Email { get; set; }
    public string Password { get; set; }
}
