using AgendaNovo.Controles;
using AgendaNovo.Interfaces;
using AgendaNovo.Models;
using AgendaNovo.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AgendaNovo.ViewModels
{
    public partial class NotificacaoViewModel : ObservableObject
    {
        public ObservableCollection<Notificacao> Notificacoes { get; } = new();
        private readonly IServicoService _servicoservice;
        private readonly IAgendamentoService _agendamentoService;
        public int CountHoje => HojeView?.Cast<Notificacao>().Count() ?? 0;
        public int CountAmanha => AmanhaView?.Cast<Notificacao>().Count() ?? 0;
        public int CountTratar => TratamentoView?.Cast<Notificacao>().Count() ?? 0;
        public int CountRevelar => RevelarView?.Cast<Notificacao>().Count() ?? 0;
        public int CountEnviar => EntregaView?.Cast<Notificacao>().Count() ?? 0;

        public int CountHojeEAmanha => CountHoje + CountAmanha;
        public int CountFotos => CountTratar + CountRevelar + CountEnviar;
        private readonly CollectionViewSource _cvsHoje;
        private readonly CollectionViewSource _cvsAmanha;
        private readonly CollectionViewSource _cvsTrat;
        private readonly CollectionViewSource _cvsRev;
        private readonly CollectionViewSource _cvsEnt;
        public ICollectionView HojeView { get; }
        public ICollectionView AmanhaView { get; }
        public ICollectionView TratamentoView { get; }
        public ICollectionView RevelarView { get; }
        public ICollectionView EntregaView { get; }

        public NotificacaoViewModel(IServicoService servicoservice, IAgendamentoService agendamentoservice)
        {
            _servicoservice = servicoservice;
            _agendamentoService = agendamentoservice;           

            var hoje = DateTime.Today;
            var amanha = hoje.AddDays(1);
            _cvsHoje = new CollectionViewSource { Source = Notificacoes };
            _cvsAmanha = new CollectionViewSource { Source = Notificacoes };
            _cvsTrat = new CollectionViewSource { Source = Notificacoes };
            _cvsRev = new CollectionViewSource { Source = Notificacoes };
            _cvsEnt = new CollectionViewSource { Source = Notificacoes };
            HojeView = _cvsHoje.View;
            AmanhaView = _cvsAmanha.View;
            TratamentoView = _cvsTrat.View;
            RevelarView = _cvsRev.View;
            EntregaView = _cvsEnt.View;


            HojeView.Filter = o => o is Notificacao n && n.AtrasoTipo == null && n.DataAgendamento.Date == hoje && !string.IsNullOrWhiteSpace(n.Titulo);
            HojeView.SortDescriptions.Add(new SortDescription(nameof(Notificacao.Horario), ListSortDirection.Ascending));

            AmanhaView.Filter = o => o is Notificacao n && n.AtrasoTipo == null && n.DataAgendamento.Date == amanha && !string.IsNullOrWhiteSpace(n.Titulo);
            AmanhaView.SortDescriptions.Add(new SortDescription(nameof(Notificacao.Horario), ListSortDirection.Ascending));

            TratamentoView.Filter = o => (o as Notificacao)?.AtrasoTipo == FotoAtrasoTipo.Tratamento;
            TratamentoView.SortDescriptions.Add(new SortDescription(nameof(Notificacao.Previsto), ListSortDirection.Ascending));

            RevelarView.Filter = o => (o as Notificacao)?.AtrasoTipo == FotoAtrasoTipo.Revelar;
            RevelarView.SortDescriptions.Add(new SortDescription(nameof(Notificacao.Previsto), ListSortDirection.Ascending));

            EntregaView.Filter = o => (o as Notificacao)?.AtrasoTipo == FotoAtrasoTipo.Entrega;
            EntregaView.SortDescriptions.Add(new SortDescription(nameof(Notificacao.Previsto), ListSortDirection.Ascending));

            // Carrega dados
            CarregarNotificacoes();
            Notificacoes.CollectionChanged += (_, __) =>
            {
                OnPropertyChanged(nameof(CountHoje));
                OnPropertyChanged(nameof(CountAmanha));
                OnPropertyChanged(nameof(CountHojeEAmanha));
                OnPropertyChanged(nameof(CountTratar));
                OnPropertyChanged(nameof(CountRevelar));
                OnPropertyChanged(nameof(CountEnviar));
                OnPropertyChanged(nameof(CountFotos));

            };
        }
        
       
            [ObservableProperty]
            private bool popupVisivel = true;
            public void CarregarNotificacoes()
            {
                Notificacoes.Clear();

                // 3.1) As suas notificações existentes (ex.: clientes de amanhã)…
                AdicionarClientesDeAmanha();

                    // 3.2) Fotos atrasadas
                    AdicionarFotosAtrasadas();
                for (int i = 0; i < Notificacoes.Count; i++)
                    Debug.WriteLine($"{i}: Tipo={Notificacoes[i]?.GetType().Name ?? "NULL"} Titulo='{Notificacoes[i]?.Titulo}'");

                PopupVisivel = Notificacoes.Count > 0;
            }
            private void AdicionarClientesDeAmanha()
            {
                var amanha = DateTime.Today.AddDays(1);
                var hoje = DateTime.Today;
                var agsHoje = _agendamentoService.GetByDate(hoje);
                foreach (var a in _agendamentoService.GetByDate(hoje))
                {
                    Notificacoes.Add(new Notificacao
                    {
                        AgendamentoId = a.Id,
                        Titulo = $"{a.Cliente?.Nome} - {a.Crianca?.Nome} {a?.MesversarioFormatado}",
                        NomeCliente = a.Cliente?.Nome,
                        Mensagem = $"{a.Data:dd/MM} às {a.Horario:hh\\:mm} • {a.Servico?.Nome}",
                        DataAgendamento = a.Data,
                        Horario = a.Horario,
                        AtrasoTipo = null,
                        Previsto = null
                    });
                }
                foreach (var a in _agendamentoService.GetByDate(amanha))
                {
                    Notificacoes.Add(new Notificacao
                    {
                        AgendamentoId = a.Id,
                        Titulo = $"{a.Cliente?.Nome} - {a.Crianca?.Nome} {a?.MesversarioFormatado}",
                        NomeCliente = a.Cliente?.Nome,
                        Mensagem = $"{a.Data:dd/MM} às {a.Horario:hh\\:mm} • {a.Servico?.Nome}",
                        DataAgendamento = a.Data,
                        Horario = a.Horario,
                        AtrasoTipo = null,
                        Previsto = null
                    });
                }
            }

            private void AdicionarFotosAtrasadas()
            {
                var hoje = DateTime.Today;
                var atrasados = _agendamentoService.GetAgendamentosComFotosAtrasadas(hoje);

                foreach (var dto in atrasados)
                {
                    foreach (var (tipo, previsto) in dto.Atrasos)
                    {
                        var dias = (hoje - previsto.Date).Days;
                        string etapa = tipo switch
                        {
                            FotoAtrasoTipo.Tratamento => "Tratamento",
                            FotoAtrasoTipo.Revelar => "Revelar",
                            FotoAtrasoTipo.Entrega => "Entrega",
                            _ => "Etapa"
                        };

                        Notificacoes.Add(new Notificacao
                        {
                            AgendamentoId = dto.Agendamento.Id,
                            Titulo = $"{dto.Agendamento.Cliente?.Nome} — {etapa} atrasado",
                            Mensagem = $"Previsto: {previsto:dd/MM} • {dias} dia(s) de atraso",
                            DataAgendamento = dto.Agendamento.Data,
                            AtrasoTipo = tipo,
                            Previsto = previsto
                        });
                    }
                }
            }

            [RelayCommand]
            private void EnviarMensagem(Notificacao notificacao)
            {

            if (notificacao.AtrasoTipo == null)
            {
                notificacao.Enviada = true;


                if (notificacao != null)
                {
                    Notificacoes.Remove(notificacao);
                }

                if (Notificacoes.Count == 0) PopupVisivel = false;

                var agendamento = _agendamentoService.GetById(notificacao.AgendamentoId);
                if (agendamento != null)
                    EnviarMensagemWhatsapp(agendamento);
                return;
            }

            // Para fotos atrasadas: marcar etapa + abrir WA
            var ag = _agendamentoService.GetById(notificacao.AgendamentoId);
            if (ag == null) return;

            var etapa = notificacao.AtrasoTipo switch
            {
                FotoAtrasoTipo.Tratamento => EtapaFotos.Tratamento,
                FotoAtrasoTipo.Revelar => EtapaFotos.Revelar,
                FotoAtrasoTipo.Entrega => EtapaFotos.Entrega,
                _ => EtapaFotos.Tratamento
            };

            // 1) Persistir a etapa como concluída hoje
            _agendamentoService.AddOrUpdateEtapa(ag.Id, etapa, DateTime.Today,
                $"(via notificação) Atraso resolvido em {DateTime.Today:dd/MM}");


            // 3) Remover notificação
            notificacao.Enviada = true;
            if (notificacao != null)
                Notificacoes.Remove(notificacao);
            if (Notificacoes.Count == 0) PopupVisivel = false;
        }
       
        public void EnviarMensagemWhatsapp(Agendamento agendamento)
        {
            if (agendamento == null || agendamento.Cliente == null)
                return;

            var cliente = agendamento.Cliente;

            // Aqui você pode personalizar o texto da mensagem
            var mensagem = $"Olá {cliente.Nome}, tudo bem?\n\n" +
                           $"Estou passando para confirmar o agendamento de amanhã ({agendamento.Data:dd/MM} às {agendamento.Horario:hh\\:mm}).\n" +
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
