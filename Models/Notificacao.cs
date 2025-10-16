using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgendaNovo.Models
{
    public class Notificacao
    {
        public int AgendamentoId { get; set; }
        public string Titulo { get; set; } = "";
        public string Mensagem { get; set; } = "";
        public string NomeCliente { get; set; } = "";
        public DateTime DataAgendamento { get; set; }
        public bool Enviada { get; set; }
        public TimeSpan? Horario { get; set; }
        public FotoAtrasoTipo? AtrasoTipo { get; set; } // null para outros tipos (ex: “amanhã”)
        public DateTime? Previsto { get; set; }
        public int GrupoOrdem => AtrasoTipo == null ? 0 : 1;
        public string SubGrupo => AtrasoTipo switch
        {
            null => "|Agendamentos",
            FotoAtrasoTipo.Tratamento => "1|Tratamento",
            FotoAtrasoTipo.Revelar => "2|Revelar",
            FotoAtrasoTipo.Entrega => "3|Entrega",
            _ => "9|Outros"
        };
        public string GrupoKey => AtrasoTipo == null ? "|Agendamentos" : "|Fotos atrasadas";
        public DateTime OrdenacaoDataHora =>
      AtrasoTipo == null
          ? (new DateTime(DataAgendamento.Year, DataAgendamento.Month, DataAgendamento.Day)
              + (Horario ?? TimeSpan.Zero))
          : (Previsto?.Date ?? DateTime.MaxValue);
    }
    public class AgendamentoAtrasoDTO
    {
        public Agendamento Agendamento { get; set; } = null!;
        public List<(FotoAtrasoTipo Tipo, DateTime Previsto)> Atrasos { get; set; } = new();
    }
}
