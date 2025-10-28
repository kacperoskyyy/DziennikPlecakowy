using System.Text.Json.Serialization;

namespace DziennikPlecakowy.DTO.Local;
//DTO podsumowania wycieczki
public class TripSummaryDTO
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("userId")]
    public string UserId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("tripDate")]
    public DateTime TripDate { get; set; }

    [JsonPropertyName("distance")]
    public double Distance { get; set; }

    [JsonPropertyName("duration")]
    public double Duration { get; set; }

    [JsonPropertyName("elevationGain")]
    public double ElevationGain { get; set; }

    [JsonPropertyName("steps")]
    public int Steps { get; set; }
}