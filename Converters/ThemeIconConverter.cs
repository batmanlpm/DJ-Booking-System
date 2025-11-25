using System;
using System.Globalization;
using System.Windows.Data;
using DJBookingSystem.Services;

namespace DJBookingSystem.Converters
{
    /// <summary>
    /// Converts icon names to theme-appropriate icon strings
    /// </summary>
    public class ThemeIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return "?";

            string? iconName = parameter.ToString();
            
            // Use IconService to get theme-appropriate icon
            return IconService.GetIcon(iconName ?? "?");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
