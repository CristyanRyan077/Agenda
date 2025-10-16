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
    public class EtapaLookupConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length < 2 || values[0] is not IEnumerable<AgendamentoEtapa> etapas)
                return string.Empty;

            var etapaEnum = (EtapaFotos)values[1];
            var e = etapas.FirstOrDefault(x => x.Etapa == etapaEnum);
            if (e is null) return string.Empty;

            string mode = parameter?.ToString() ?? "full";
            bool onlyObs = mode.Contains("obs", StringComparison.OrdinalIgnoreCase);
            bool onlyDate = mode.Contains("date", StringComparison.OrdinalIgnoreCase);

            // trunc param
            int? trunc = null;

            var part = mode.Split(';').FirstOrDefault(s => s.StartsWith("t:", StringComparison.OrdinalIgnoreCase));
            if (part != null && int.TryParse(part.AsSpan(2), out var n) && n > 0) trunc = n;

            string data = e.DataConclusao.ToString("dd/MM");
            string obs = e.Observacao ?? string.Empty;

            string result =
                onlyObs ? obs :
                onlyDate ? data :
                string.IsNullOrWhiteSpace(obs) ? data : $"{data} — {obs}";

            if (trunc.HasValue && result.Length > trunc.Value)
                result = result.Substring(0, trunc.Value) + "…";

            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

}
