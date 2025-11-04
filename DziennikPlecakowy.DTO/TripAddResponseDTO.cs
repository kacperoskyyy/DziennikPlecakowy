namespace DziennikPlecakowy.DTO;
// DTO dla odpowiedzi po dodaniu wycieczki
public class TripAddResponseDTO
{
    [System.Text.Json.Serialization.JsonPropertyName("tripId")]
    public string TripId { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("message")]
    public string Message { get; set; }
}