using System;
using System.Windows;
using System.Globalization;

namespace SRO.ViewModels
{
    /// <summary>
    /// A converter that takes in a boolean and returns a <see cref="Visibility"/>
    /// </summary>
    public class BooleanToVisibilityConverter : BaseValueConverter<BooleanToVisibilityConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter != null)
                return (bool)value ? Visibility.Collapsed : Visibility.Visible;

            // If parameter is not specified, then use boolean behavior
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
