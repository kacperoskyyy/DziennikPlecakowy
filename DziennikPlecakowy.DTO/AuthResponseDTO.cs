namespace DziennikPlecakowy.DTO;

// DTO dla odpowiedzi uwierzytelniania z tokenem JWT i tokenem odświeżającym
public class AuthResponseDTO
{
    public string Token { get; set; }
    public string RefreshToken { get; set; }
}