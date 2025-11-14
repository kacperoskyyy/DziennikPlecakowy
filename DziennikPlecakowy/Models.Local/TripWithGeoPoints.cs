namespace DziennikPlecakowy.Models.Local;
public class TripWithGeoPoints
{
    public LocalTrip Trip { get; set; }
    public List<LocalGeoPoint> GeoPoints { get; set; } = new List<LocalGeoPoint>();
}
