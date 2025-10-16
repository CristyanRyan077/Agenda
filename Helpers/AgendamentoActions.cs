using AgendaNovo._01_Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgendaNovo.Helpers
{
    public sealed class AgendamentoActions : ObservableObject
    {
        public IRelayCommand<Agendamento> EditarAgendamentoCommand { get; }
        public IRelayCommand<Agendamento> PagamentosAgendamentoCommand { get; }

        public AgendamentoActions(IAgendamentoActionProvider p)
        {
            EditarAgendamentoCommand = new RelayCommand<Agendamento>(ag =>
            {
                if (ag is null) return;
                p.SelecionarAgendamento(ag);
                p.EditarAgendamentoPorId(ag.Id);
                p.HistoricoCliente(ag.ClienteId);
                p.AplicarDestaqueNoHistorico();
            });

            PagamentosAgendamentoCommand = new RelayCommand<Agendamento>(ag =>
            {
                if (ag is null) return;
                p.SelecionarAgendamento(ag);
                _ = p.AbrirPagamentosAsync(ag.Id);
                p.HistoricoCliente(ag.ClienteId);
                p.AplicarDestaqueNoHistorico();
            });
        }
    }
}
