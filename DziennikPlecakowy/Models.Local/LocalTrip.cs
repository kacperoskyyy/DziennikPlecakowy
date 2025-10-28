using SQLite;

namespace DziennikPlecakowy.Models.Local;
// lokalny model wycieczki

[Table("trips")]
public class LocalTrip
{
    [PrimaryKey, AutoIncrement]
    public long LocalId { get; set; }

    [Indexed]
    public string ServerId { get; set; }

    [Indexed]
    public string UserId { get; set; }

    public string Name { get; set; }
    public DateTime TripDate { get; set; }
    public double Distance { get; set; }
    public double Duration { get; set; }
    public double ElevationGain { get; set; }
    public long Steps { get; set; }
    public bool IsSynchronized { get; set; } = false;
}