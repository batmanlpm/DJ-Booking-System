using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DJBookingSystem.Converters
{
    /// <summary>
    /// Converts online status (bool) to color for status box
    /// True (Online) = Green, False (Offline) = Red
    /// </summary>
    public class BoolToStatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isOnline)
            {
                // Green for online, Red for offline
                return isOnline 
                    ? new SolidColorBrush(Color.FromRgb(0, 255, 0))    // #00FF00 Neon Green
                    : new SolidColorBrush(Color.FromRgb(255, 0, 0));   // #FF0000 Red
            }
            
            // Default to gray if unknown
            return new SolidColorBrush(Color.FromRgb(128, 128, 128));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
