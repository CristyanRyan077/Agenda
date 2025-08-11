using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace AgendaNovo.Converters
{
    public class PagoParaCorBrushConverter : IValueConverter
    {
        public Brush Pago { get; set; } = new SolidColorBrush(Color.FromRgb(0x2E, 0x7D, 0x32));   // verde
        public Brush Pendente { get; set; } = new SolidColorBrush(Color.FromRgb(0xEF, 0x6C, 0x00)); // laranja

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var b = value as bool?;  // caso seja bool?
            return (b ?? false) ? Pago : Pendente;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
}
