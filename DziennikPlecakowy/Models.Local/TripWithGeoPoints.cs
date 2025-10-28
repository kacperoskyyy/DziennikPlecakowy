namespace DziennikPlecakowy.Models.Local;
// lokalny model wycieczki wraz z punktami geograficznymi
public class TripWithGeoPoints
{
    public LocalTrip Trip { get; set; }
    public List<LocalGeoPoint> GeoPoints { get; set; } = new List<LocalGeoPoint>();
}
