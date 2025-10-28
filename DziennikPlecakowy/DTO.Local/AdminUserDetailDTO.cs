using DziennikPlecakowy.Models;
using System.Text.Json.Serialization;

namespace DziennikPlecakowy.DTO.Local;

// DTO do odbioru listy userów
public class AdminUserDetailDTO
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("isLocked")]
    public bool IsLocked { get; set; }

    [JsonPropertyName("mustChangePassword")]
    public bool MustChangePassword { get; set; }

    [JsonPropertyName("roles")]
    public List<UserRole> Roles { get; set; }
}