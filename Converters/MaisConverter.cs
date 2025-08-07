using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace AgendaNovo.Converters
{
    public class MaisConverter : IValueConverter
    {
        // Quantos itens mostrar antes de começar a contar "mais"
        public int Limit { get; set; } = 3;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count && count > Limit)
                return $"+{count - Limit} mais";
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    // Define visibilidade apenas se Count > Limit
    public class MaisVisibilityConverter : IValueConverter
    {
        public int Limit { get; set; } = 3;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count && count > Limit)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
