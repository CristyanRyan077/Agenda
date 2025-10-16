using AgendaNovo.Models;
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
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = (StatusAgendamento)value;
            var target = parameter?.ToString();

            if (status == StatusAgendamento.Confirmado && target == "Confirmado")
                return Brushes.Green;
            if (status == StatusAgendamento.Cancelado && target == "Cancelado")
                return Brushes.Red;
            return Brushes.LightGray; // padrão (pendente)
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
