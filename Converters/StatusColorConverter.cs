using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PupTrailsV3.Converters
{
    public class StatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return new SolidColorBrush(Colors.Gray);

            string status = value.ToString()?.ToLower() ?? "";

            return status switch
            {
                "in care" => new SolidColorBrush(Color.FromRgb(76, 175, 80)),      // Green
                "ready" => new SolidColorBrush(Color.FromRgb(33, 150, 243)),       // Blue
                "adopted" => new SolidColorBrush(Color.FromRgb(156, 39, 176)),     // Purple
                "planned" => new SolidColorBrush(Color.FromRgb(255, 152, 0)),      // Orange
                "in transport" => new SolidColorBrush(Color.FromRgb(3, 169, 244)), // Light Blue
                "vet pending" => new SolidColorBrush(Color.FromRgb(255, 193, 7)),  // Amber
                "transferred" => new SolidColorBrush(Color.FromRgb(158, 158, 158)),// Gray
                "deceased" => new SolidColorBrush(Color.FromRgb(244, 67, 54)),     // Red
                _ => new SolidColorBrush(Color.FromRgb(158, 158, 158))             // Default Gray
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
