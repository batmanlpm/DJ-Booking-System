using System;
using System.Globalization;
using System.Windows.Data;

namespace DJBookingSystem.Converters
{
    /// <summary>
    /// Converts boolean value to status text (Open/Closed)
    /// </summary>
    public class BoolToStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isOpen)
            {
                return isOpen ? "OPEN" : "CLOSED";
            }
            return "UNKNOWN";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
