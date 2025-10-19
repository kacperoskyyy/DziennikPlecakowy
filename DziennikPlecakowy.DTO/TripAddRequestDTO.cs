using DziennikPlecakowy.Models;

namespace DziennikPlecakowy.DTO;

// DTO dla dodawania nowej wycieczki
public class TripAddRequestDTO
{
    public DateTime TripDate { get; set; }
    public double Distance { get; set; }
    public double Duration { get; set; }
    public GeoPoint[] GeoPointList { get; set; }
    public string Name { get; set; }
    public double ElevationGain { get; set; }
    public int Steps { get; set; }

}
