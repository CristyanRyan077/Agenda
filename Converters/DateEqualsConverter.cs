using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AgendaNovo.Converters
{
    public class DateEqualsConverter : IMultiValueConverter
    {
        private static DateTime? ToDate(object v)
        {
            if (v == null) return null;

            // v pode vir como DateTime (value), DateTime? (boxed) etc.
            if (v is DateTime d) return d.Date;

            // “as” funciona com tipos anuláveis (Nullable<DateTime>)
            var n = v as DateTime?;
            if (n.HasValue) return n.Value.Date;

            return null;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2) return false;

            var a = ToDate(values[0]);
            var b = ToDate(values[1]);

            return a.HasValue && b.HasValue && a.Value == b.Value;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

}
