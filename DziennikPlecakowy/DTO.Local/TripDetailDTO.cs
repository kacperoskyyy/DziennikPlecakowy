using System.Text.Json.Serialization;

namespace DziennikPlecakowy.DTO.Local;

//DTO detali wycieczki
public class TripDetailDTO
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

    [JsonPropertyName("geopointList")]
    public List<GeoPointDTO> GeoPointList { get; set; } = new List<GeoPointDTO>();
}