using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgendaNovo._01_Interfaces
{
    public interface IAgendamentoActionProvider
    {
        void SelecionarAgendamento(Agendamento ag);
        void EditarAgendamentoPorId(int id);
        Task AbrirPagamentosAsync(int id);
        void HistoricoCliente(int clienteId);
        void AplicarDestaqueNoHistorico();
    }
}
