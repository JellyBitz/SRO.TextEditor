using System;
using System.Globalization;
using System.Windows.Media;

namespace SRO.ViewModels
{
    /// <summary>
    /// Converts a Color into a SolidBrush.
    /// </summary>
    public class ColorToBrushConverter : BaseValueConverter<ColorToBrushConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value != null && value is Color color)
                return new SolidColorBrush(color);
            return Brushes.Transparent;
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is SolidColorBrush brush)
                return brush.Color;
            return Colors.Transparent;
        }
    }
}
