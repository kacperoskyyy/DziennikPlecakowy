namespace DziennikPlecakowy.DTO;

public class UserDetailDTO
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime? LastLoginTime { get; set; }
    public List<string> Roles { get; set; }
}