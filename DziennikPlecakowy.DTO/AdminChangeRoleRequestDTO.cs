using DziennikPlecakowy.Models;

namespace DziennikPlecakowy.DTO;

public class AdminChangeRoleRequestDTO
{
    public string UserId { get; set; }
    public UserRole NewRole { get; set; }
}