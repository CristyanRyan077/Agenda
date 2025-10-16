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
    public class EtapaToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = value is EtapaStatus s ? s : EtapaStatus.Pendente;
            // cores: cinza (pendente), amarelo (hoje), vermelho (atrasado), verde (concluído)
            return status switch
            {
                EtapaStatus.Concluido => Brushes.MediumSeaGreen,
                EtapaStatus.Hoje => Brushes.Gold,
                EtapaStatus.Atrasado => Brushes.IndianRed,
                _ => Brushes.LightGray
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
