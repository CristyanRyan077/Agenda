using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgendaNovo.Helpers
{
    public class Formatar
    {
        public static string? FormatMesversario(int? meses)
        {
            if (!meses.HasValue) return null;
            if (meses.Value < 12)
                return $"{meses.Value} {(meses.Value == 1 ? "mês" : "meses")}";

            int anos = meses.Value / 12;
            int mesesRestantes = meses.Value % 12;

            if (mesesRestantes == 0)
                return $"{anos} {(anos == 1 ? "ano" : "anos")}";

            return $"{anos} {(anos == 1 ? "ano" : "anos")} e {mesesRestantes} {(mesesRestantes == 1 ? "mês" : "meses")}";
        }
    }
}
