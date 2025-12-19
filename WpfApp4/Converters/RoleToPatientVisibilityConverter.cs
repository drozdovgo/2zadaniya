using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WpfApp4.Converters
{
    public class RoleToPatientVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() == "пациент" ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}