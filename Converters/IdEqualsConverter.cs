using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AgendaNovo.Converters
{
    public class IdEqualsConverter : IMultiValueConverter
    {
        private static int? ToInt(object v)
        {
            if (v == null) return null;

            if (v is int i) return i;              // int "seco"
            var ni = v as int?;                    // Nullable<int> boxed
            if (ni.HasValue) return ni.Value;

            // opcional: tentar parsear string
            if (v is string s && int.TryParse(s, out var p)) return p;

            return null;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2) return false;

            var a = ToInt(values[0]);
            var b = ToInt(values[1]);

            return a.HasValue && b.HasValue && a.Value == b.Value;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
