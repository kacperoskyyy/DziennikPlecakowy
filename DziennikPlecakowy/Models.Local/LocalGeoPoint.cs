using SQLite;

namespace DziennikPlecakowy.Models.Local;

[Table("geo_points")]
public class LocalGeoPoint
{
    [PrimaryKey, AutoIncrement]
    public long LocalGeoPointId { get; set; }

    [Indexed]
    public long LocalTripId { get; set; }

    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Height { get; set; }
    public DateTime Timestamp { get; set; }
}