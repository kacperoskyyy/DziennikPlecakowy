using DziennikPlecakowy.Models;

namespace DziennikPlecakowy.DTO;
public class ChangeRoleRequestDTO
{
    public string UserId { get; set; }
    public UserRole NewRole { get; set; }
}
