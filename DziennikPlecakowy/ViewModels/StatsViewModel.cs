namespace DziennikPlecakowy.ViewModels
{
    public class StatsViewModel : BaseViewModel
    {
        public int TotalTrips { get; set; } = 0;
        public double TotalDistance { get; set; } = 0;
        public double TotalDuration { get; set; } = 0;

        public StatsViewModel()
        {
           
        }
    }
}
