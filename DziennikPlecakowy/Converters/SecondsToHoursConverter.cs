using System.Globalization;

namespace DziennikPlecakowy.Converters;
public class SecondsToHoursConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double seconds)
        {
            double hours = seconds / 3600.0;
            return $"{hours:F2} h";
        }
        return "0.00 h";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}