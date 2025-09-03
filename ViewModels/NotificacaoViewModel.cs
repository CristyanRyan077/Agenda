using AgendaNovo.Controles;
using AgendaNovo.Interfaces;
using AgendaNovo.Models;
using AgendaNovo.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AgendaNovo.ViewModels
{
    public partial class NotificacaoViewModel : ObservableObject
    {
        public ObservableCollection<Notificacao> Notificacoes { get; } = new();
        private readonly IServicoService _servicoservice;
        private readonly IAgendamentoService _agendamentoService;
        public NotificacaoViewModel (IServicoService servicoservice, IAgendamentoService agendamentoservice)
        {
            _servicoservice = servicoservice;
            _agendamentoService = agendamentoservice;
        }
        [ObservableProperty]
        private bool popupVisivel = true;

        [RelayCommand]
        private void EnviarMensagem(Notificacao notificacao)
        {
            
            notificacao.Enviada = true;
            Notificacoes.Remove(notificacao);
            if (Notificacoes.Count == 0)
                PopupVisivel = false;
            var agendamento = _agendamentoService.GetById(notificacao.AgendamentoId);
            if (agendamento != null)
                EnviarMensagemWhatsapp(agendamento);
        }
        public void EnviarMensagemWhatsapp(Agendamento agendamento)
        {
            if (agendamento == null || agendamento.Cliente == null)
                return;

            var cliente = agendamento.Cliente;

            // Aqui você pode personalizar o texto da mensagem
            var mensagem = $"Olá {cliente.Nome}, tudo bem?\n\n" +
                           $"Estou passando para confirmar o agendamento de amanhã ({agendamento.Data:dd/MM} às {agendamento.Horario}h).\n" +
                           $"Qualquer dúvida, me avise 🙂";

            var textoEscapado = Uri.EscapeDataString(mensagem);

            string telefoneFormatado = $"55859{Regex.Replace(cliente.Telefone, @"\D", "")}";
            string url = $"https://web.whatsapp.com/send?phone={telefoneFormatado}&text={textoEscapado}";

            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        [RelayCommand]
        private void AbrirDetalhes()
        {
            var detalhesWindow = new PopupDetalhes
            {
                DataContext = this
            };
            detalhesWindow.Show();
        }
        [RelayCommand]
        private void FecharPopup()
        {
            PopupVisivel = false;
        }
    }
}
