using System.Globalization;

namespace DziennikPlecakowy.Converters;

public class IsNotNullOrEmptyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isNotEmpty = !string.IsNullOrEmpty(value as string);

        if (parameter is string s && string.Equals(s, "Inverted", StringComparison.OrdinalIgnoreCase))
        {
            return !isNotEmpty;
        }

        return isNotEmpty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}