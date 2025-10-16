using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AgendaNovo.Converters
{
    public class NotificacaoGrupoConverter : IValueConverter
    {
        // value = FotoAtrasoTipo? (null para "amanhã")
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "Amanhã";
            return "Fotos atrasadas";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
    public class NotificacaoSubGrupoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // value = FotoAtrasoTipo? (null para "amanhã")
            string s = value?.ToString();
            return s switch
            {
                null => "0|Agendamentos",
                "Tratamento" => "1|Tratamento",
                "Revelar" => "2|Revelar",
                "Entrega" => "3|Entrega",
                _ => "9|Outros"
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    // Mostra apenas a parte após o pipe no header do subgrupo
    public class GroupNameDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var name = value?.ToString() ?? "";
            var idx = name.IndexOf('|');
            return idx >= 0 ? name[(idx + 1)..] : name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
