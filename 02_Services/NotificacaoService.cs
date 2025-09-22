using AgendaNovo.Interfaces;
using AgendaNovo.Models;
using AgendaNovo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AgendaNovo.Services
{
    public class NotificacaoService : INotificacaoService
    {
        private readonly NotificacaoViewModel _vm;
        private readonly IAgendamentoService _agservice;
        private readonly HashSet<int> _notificacoesExibidas = new();

        public NotificacaoService(NotificacaoViewModel vm, IAgendamentoService agservice)
        {
            _vm = vm;
            _agservice = agservice;

            var timer = new System.Timers.Timer(600000); // 10 min
            timer.Elapsed += (s, e) => VerificarAgendamentos();
            timer.Start();

            VerificarAgendamentos();
        }
        public List<Agendamento> GetAgendamentosParaAmanha()
        {
            var dataAlvo = DateTime.Now.AddDays(1).Date; 
            return _agservice.GetByDate(dataAlvo);
        }
        public void VerificarAgendamentos()
        {
            var agendamentos = GetAgendamentosParaAmanha();
            foreach (var ag in agendamentos)
            {
                if (!_notificacoesExibidas.Contains(ag.Id))
                {
                    var notif = new Notificacao
                    {
                        AgendamentoId = ag.Id,
                        NomeCliente = ag.Cliente.Nome,
                        DataAgendamento = ag.Data
                    };
                    Application.Current.Dispatcher.Invoke(() => _vm.Notificacoes.Add(notif));
                    _notificacoesExibidas.Add(ag.Id);
                }
            }
        }

    }
}
