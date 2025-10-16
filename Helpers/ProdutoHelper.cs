using AgendaNovo.Models;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AgendaNovo.Agendamento;

namespace AgendaNovo.Helpers
{
    public class ProdutoHelper
    {
        public static int ResolvePrazoDias(AgendamentoProduto ap)
        {
            if (ap.PrazoProducaoDias > 0) return (int)ap.PrazoProducaoDias;
            return ap.Produto?.PrazoProducaoDias ?? 0;
        }

        public static DateTime CalculaPrevisto(AgendamentoProduto ap)
        {
            var dias = ResolvePrazoDias(ap);
            var baseDate = ap.EnviadoParaProducaoEm?.Date
                           ?? ap.Agendamento.Data.Date; // fallback
            return BusinessDays.AddBusinessDays(baseDate, dias);
        }

        public static EtapaStatus CalculaStatusProduto(AgendamentoProduto ap, DateTime hoje)
        {
            if (ap.ProducaoConcluidaEm.HasValue) return EtapaStatus.Concluido;

            if (!ap.EnviadoParaProducaoEm.HasValue) return EtapaStatus.Pendente;

            var previsto = CalculaPrevisto(ap).Date;
            if (hoje.Date > previsto) return EtapaStatus.Atrasado;
            if (hoje.Date == previsto) return EtapaStatus.Hoje;
            return EtapaStatus.Pendente;
        }
    } 
}
