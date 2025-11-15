using System.Globalization;

namespace DziennikPlecakowy.Converters;
public class SecondsToHoursConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double seconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(seconds);

            int totalHours = (int)time.TotalHours;
            int minutes = time.Minutes;

            return $"{totalHours}h {minutes}m";
        }

        return "0h 0m";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}