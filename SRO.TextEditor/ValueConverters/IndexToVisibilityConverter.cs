using System;
using System.Globalization;
using System.Windows;

namespace SRO.ViewModels
{
    /// <summary>
    /// Set as visible if the value is the same as the parameter.
    /// </summary>
    public class IndexToVisibilityConverter : BaseValueConverter<IndexToVisibilityConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value != null && parameter != null)
            {
                if (int.TryParse(value.ToString(), out var _value) && int.TryParse(parameter.ToString(), out var _index))
                {
                    return _value == _index ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            // Always visible as default behavior
            return Visibility.Collapsed;
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
