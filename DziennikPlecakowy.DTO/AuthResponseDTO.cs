namespace DziennikPlecakowy.DTO;

public class AuthResponseDTO
{
    public string Token { get; set; }
    public string RefreshToken { get; set; }
    public bool MustChangePassword { get; set; }
}