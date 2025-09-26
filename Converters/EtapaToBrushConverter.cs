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
    public class EtapaToBrushConverter : IValueConverter
    {
        public Brush ConcluidoBrush { get; set; } = Brushes.Green;
        public Brush PendenteBrush { get; set; } = Brushes.Gray;
        public Brush AtrasadoBrush { get; set; } = Brushes.Red;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool concluido && concluido) return ConcluidoBrush;
            if (value is string status && status == "Atrasado") return AtrasadoBrush;
            return PendenteBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
