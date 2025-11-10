using System.ComponentModel.DataAnnotations;
namespace DziennikPlecakowy.DTO;
public class UserChangePasswordRequestDTO
{
    public string? Password { get; set; }

    [Required(ErrorMessage = "Nowe hasło jest wymagane.")]
    [MinLength(6, ErrorMessage = "Hasło musi mieć co najmniej 6 znaków.")]
    public string NewPassword { get; set; }
}