using System;
using System.Globalization;
using System.Windows.Data;

namespace WpfApp4.Converters
{
    public class StatusToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status switch
                {
                    "запланирована" => "📅",
                    "завершена" => "✅",
                    "отменена" => "❌",
                    _ => "❓"
                };
            }
            return "❓";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}