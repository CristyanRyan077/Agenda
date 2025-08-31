using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgendaNovo.ViewModels
{
    public class DadosAtualizadosMessage
    {
        public int? ClienteId { get; }
        public int? CriancaId { get; }

        public int? AgendamentoId { get; }

        public DadosAtualizadosMessage(int? clienteId = null, int? criancaId = null, int? agendamentoId = null)
        {
            ClienteId = clienteId;
            CriancaId = criancaId;
            AgendamentoId = agendamentoId;
        }
    }
}
