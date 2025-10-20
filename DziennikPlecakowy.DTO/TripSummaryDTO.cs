namespace DziennikPlecakowy.DTO;

public class TripSummaryDTO
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string Name { get; set; }
    public DateTime TripDate { get; set; }
    public double Distance { get; set; }
    public double Duration { get; set; }
    public double ElevationGain { get; set; }
    public int Steps { get; set; }
}