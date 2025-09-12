using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AgendaNovo.Converters
{
    public class DecimalSafeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal d) return d.ToString(culture);
            if (value is null) return string.Empty;
            return System.Convert.ToString(value, culture) ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = (value as string)?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(s)) return 0m;

            // estados transitórios comuns
            if (s == "." || s == ",") return 0m;

            // tenta com a cultura atual
            if (decimal.TryParse(s, NumberStyles.Number, culture, out var d))
                return d;

            // tenta com o separador alternativo
            var altSep = culture.NumberFormat.NumberDecimalSeparator == "," ? "." : ",";
            var curSep = culture.NumberFormat.NumberDecimalSeparator;
            var alt = s.Replace(altSep, curSep);
            if (decimal.TryParse(alt, NumberStyles.Number, culture, out d))
                return d;

            // não propaga exceção de binding
            return Binding.DoNothing;
        }
    }
}
