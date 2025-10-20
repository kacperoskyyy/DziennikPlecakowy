namespace DziennikPlecakowy.DTO;

public class AdminUserDetailDTO
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public bool IsLocked { get; set; }
    public bool MustChangePassword { get; set; }
    public List<string> Roles { get; set; }
}