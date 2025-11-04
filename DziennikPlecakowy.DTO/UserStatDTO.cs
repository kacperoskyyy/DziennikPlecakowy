using System.Text.Json.Serialization;

namespace DziennikPlecakowy.DTO;
// DTO statystyk użytkownika
public class UserStatDTO
{
    [JsonPropertyName("tripsCount")]
    public int TripsCount { get; set; }

    [JsonPropertyName("totalDistance")]
    public double TotalDistance { get; set; }

    [JsonPropertyName("totalDuration")]
    public double TotalDuration { get; set; }

    [JsonPropertyName("totalElevationGain")]
    public double TotalElevationGain { get; set; }

    [JsonPropertyName("totalSteps")]
    public long TotalSteps { get; set; }
}