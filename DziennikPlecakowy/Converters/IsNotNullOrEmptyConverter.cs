using System.Globalization;

namespace DziennikPlecakowy.Converters;

// Converter sprawdzający, czy string NIE jest pusty

public class IsNotNullOrEmptyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Zwróć 'true', jeśli string NIE jest pusty
        return !string.IsNullOrEmpty(value as string);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}