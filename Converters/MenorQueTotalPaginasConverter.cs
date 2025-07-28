using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AgendaNovo.Converters
{
    public class MenorQueTotalPaginasConverter : IValueConverter
    {
        public MenorQueTotalPaginasConverter() { }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int paginaAtual && parameter is string totalStr && int.TryParse(totalStr, out int total))
                return paginaAtual < total;

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
