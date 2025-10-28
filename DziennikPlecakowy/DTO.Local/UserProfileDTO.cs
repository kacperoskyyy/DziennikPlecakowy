using DziennikPlecakowy.Models;
using System.Text.Json.Serialization;

namespace DziennikPlecakowy.DTO;
// DTO profilu użytkownika
public class UserProfileDTO
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("roles")]
    public List<UserRole> Roles { get; set; }

    [JsonPropertyName("stats")]
    public UserStatDTO Stats { get; set; }
}