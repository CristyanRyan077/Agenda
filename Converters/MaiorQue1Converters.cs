using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows.Data;

namespace AgendaNovo.Converters
{
    public class MaiorQue1Converter : IValueConverter
    {
        public MaiorQue1Converter() { }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is int intValue && intValue > 1;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
