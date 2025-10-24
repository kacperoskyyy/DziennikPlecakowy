using DziennikPlecakowy.Models;
using System.Text.Json.Serialization;

namespace DziennikPlecakowy.DTO
{
    public class AdminChangeRoleRequestDTO
    {
        [JsonPropertyName("userId")]
        public string UserId { get; set; }

        [JsonPropertyName("newRole")]
        public UserRole NewRole { get; set; }
    }
}