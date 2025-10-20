using System.Text.Json.Serialization;

namespace DziennikPlecakowy.DTO;
public class ErrorResponseDTO
{
    [JsonPropertyName("message")]
    public string Message { get; set; }
}