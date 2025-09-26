using AgendaNovo.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AgendaNovo.Converters
{
    public sealed class SetEtapaParam
    {
        public Agendamento Agendamento { get; init; } = null!;
        public EtapaFotos Etapa { get; init; }
    }
    public class StageParamConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // values[0] = Agendamento (DataContext do card)
            // values[1] = Etapa (passada via x:Static)
            return new SetEtapaParam
            {
                Agendamento = values[0] as Agendamento ?? throw new ArgumentNullException("Agendamento"),
                Etapa = (EtapaFotos)values[1]
            };
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
