using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AgendaNovo.Converters
{
    public class TruncateConverter : IValueConverter
    {
        public int Max { get; set; } = 60;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var max = Max;
            if (parameter is string s && int.TryParse(s, out var n)) max = n;
            var text = value?.ToString() ?? "";
            if (string.IsNullOrWhiteSpace(text)) return "";
            return text.Length <= max ? text : text.Substring(0, max) + "…";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value;
    }

}
