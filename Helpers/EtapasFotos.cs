using AgendaNovo.Models;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgendaNovo.Helpers
{
    public class EtapasFotos
    {
        public static EtapaStatus StatusTratar(Agendamento a, DateTime hoje)
        {
            if (a.TratadasEm != null) return EtapaStatus.Concluido;
            var due = a.PrevistoTratar.Date;
            if (hoje > due) return EtapaStatus.Atrasado;
            if (hoje == due) return EtapaStatus.Hoje;
            return EtapaStatus.Pendente;
        }
        public static EtapaStatus StatusProducao(Agendamento a, IEnumerable<Agendamento.AgendamentoProduto> itens, DateTime hoje)
        {
            // Se todos concluídos => Concluído
            if (itens.Any() && itens.All(i => i.ProducaoConcluida)) return EtapaStatus.Concluido;

            // Data prevista global = MAX(PrevistoProducao)
            var due = itens.Any() ? itens.Max(i => i.PrevistoProducao.Date) : a.Data.Date;
            if (hoje > due) return EtapaStatus.Atrasado;
            if (hoje == due) return EtapaStatus.Hoje;
            return EtapaStatus.Pendente;
        }
        public static EtapaStatus StatusEntrega(Agendamento a, IEnumerable<Agendamento.AgendamentoProduto> itens, DateTime hoje)
        {
            if (a.EntregueEm != null) return EtapaStatus.Concluido;

            // Prazo de entrega = quando todos os produtos devem estar prontos (MAX PrevistoProducao).
            // Se quiser um buffer adicional (ex.: +3 dias), adicione aqui.
            var due = itens.Any() ? itens.Max(i => i.PrevistoProducao.Date) : a.Data.Date;
            if (hoje > due) return EtapaStatus.Atrasado;
            if (hoje == due) return EtapaStatus.Hoje;
            return EtapaStatus.Pendente;
        }
    }
}
