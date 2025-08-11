using AgendaNovo.Controles;
using AgendaNovo.Interfaces;
using AgendaNovo.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AgendaNovo.ViewModels
{
    public partial class CalendarioViewModel : ObservableObject
    {
        public AgendaViewModel AgendaViewModel { get; }
        private readonly AgendaViewModel _agenda;
        private readonly IClienteService _clienteService;
        private readonly ICriancaService _criancaService;
        private readonly IAgendamentoService _agendamentoService;
        private readonly IServicoService _servicoService;
        private readonly IPacoteService _pacoteService;


        public CalendarioViewModel(AgendaViewModel agendaViewModel, IAgendamentoService agendamentoService,
        IClienteService clienteService,
        ICriancaService criancaService,
        IPacoteService pacoteService,
        IServicoService servicoService)
        {

            AgendaViewModel = agendaViewModel;
            _agendamentoService = agendamentoService;
            _clienteService = clienteService;
            _criancaService = criancaService;
            _pacoteService = pacoteService;
            _servicoService = servicoService;
            _clienteService.ClienteInativo();
            MesAtual = DateTime.Today;
            tipoSelecionado = TipoBusca.Cliente;
            CarregarDias();
            MoverAgendamentoCommand = new RelayCommand<(Agendamento, DateTime)>(param =>
            {
                var (ag, novaData) = param;
                // mantém o horário atual:
                TimeSpan horario = ag.Horario ?? TimeSpan.Zero;
                ag.Data = novaData;
                ag.Horario = horario;
                _agendamentoService.Update(ag);
                RefreshCalendar();
            });
            RefreshCalendar();
        }

        public IAgendamentoService AgendamentoService => _agendamentoService;
        public IClienteService ClienteService => _clienteService;
        public ICriancaService CriancaService => _criancaService;
        public IPacoteService PacoteService => _pacoteService;
        public IServicoService ServicoService => _servicoService;

        public IEnumerable<ServicoLegenda> GlossarioServicos => ServicoPalette.All;
        [ObservableProperty]
        private ObservableCollection<DiaCalendario> diasDoMes = new();
        [ObservableProperty] private ObservableCollection<Agendamento> agendamentosDoDiaSelecionado;
        public ObservableCollection<Agendamento> ListaAgendamentos { get; } = new();
        public IRelayCommand<(Agendamento ag, DateTime novaData)> MoverAgendamentoCommand { get; }
        public ObservableCollection<Crianca> ListaCriancas { get; } = new();
        [ObservableProperty] private ObservableCollection<Cliente> listaClientes = new();

        [ObservableProperty] private object telaEditarAgendamento;
        [ObservableProperty] private bool mostrarEditarAgendamento;


        [ObservableProperty]
        private DateTime mesAtual = DateTime.Today;

        [ObservableProperty] private string termoBusca;
        [ObservableProperty] private TipoBusca tipoSelecionado;
        [ObservableProperty] private Cliente clienteSelecionado;
        [ObservableProperty] private Agendamento agendamentoSelecionado;
        [ObservableProperty] private bool detalhesVisiveis;
        public void RefreshCalendar()
        {
            // 1) Recarrega só os agendamentos
            var todos = _agendamentoService.GetAll().OrderBy(a => a.Horario).ToList();
            ListaAgendamentos.Clear();
            foreach (var a in todos)
                ListaAgendamentos.Add(a);

            // 2) Atualiza cada célula DiaDoMesViewModel (só a lista de agendamentos de cada dia)
            foreach (var diaVm in DiasDoMes)
            {
                var agsNoDia = todos
                    .Where(a => a.Data.Date == diaVm.Data.Date)
                    .ToList();

                diaVm.Agendamentos.Clear();
                foreach (var a in agsNoDia)
                    diaVm.Agendamentos.Add(a);
            }


            var agendaVM = AgendaViewModel;
            agendaVM.AtualizarAgendamentos();
        }

        [RelayCommand]
        private void Buscar()
        {
            if (TipoSelecionado == TipoBusca.Cliente)
                BuscarCliente(ClienteSelecionado);
            else
                BuscarAgendamento();
        }
        public void BuscarCliente(Cliente Cliente)
        {
            var clienteId = _clienteService.GetById(Cliente.Id);
            if (clienteId != null)
            {
                ClienteSelecionado = clienteId;
                DetalhesVisiveis = true;
            }
            else
            {
                DetalhesVisiveis = false;
            }
        }
        [RelayCommand]
        private void FecharEdicao()
        {
            MostrarEditarAgendamento = false;
            TelaEditarAgendamento = null;
        }

        partial void OnClienteSelecionadoChanged(Cliente value)
        {
            AtualizarCriancasDoCliente(value);
        }
        public void EditarAgendamentoSelecionado()
        {
            if (AgendamentoSelecionado == null) return;
            var agendaVM = AgendaViewModel;
            agendaVM._populandoCampos = true;
            var agendamentoCompleto = _agendamentoService.GetById(AgendamentoSelecionado.Id);
            if (agendamentoCompleto == null) return;
            ListaCriancas.Clear();

            agendaVM.NovoAgendamento = agendamentoCompleto;
            agendaVM.NovoCliente = agendamentoCompleto.Cliente;
            agendaVM.DataSelecionada = agendamentoCompleto.Data;
            agendaVM.ClienteSelecionado = agendamentoCompleto.Cliente;
            agendaVM.CriancaSelecionada = agendamentoCompleto.Crianca;
            agendaVM.ItemSelecionado = agendamentoCompleto;

            agendaVM.HorarioTexto = agendamentoCompleto.Horario?.ToString(@"hh\:mm");
            agendaVM.CarregarServicos();
            agendaVM.CarregarPacotes();
        if (agendamentoCompleto.ServicoId.HasValue)
            agendaVM.FiltrarPacotesPorServico(agendamentoCompleto.ServicoId.Value);
            agendaVM.PreencherValorPacoteSelecionado(agendaVM.NovoAgendamento.PacoteId);


            agendaVM.ServicoSelecionado = agendaVM.ListaServicos.FirstOrDefault(s => s.Id == agendaVM.NovoAgendamento.ServicoId);
            agendaVM.Pacoteselecionado = agendaVM.ListaPacotesFiltrada.FirstOrDefault(p => p.Id == agendaVM.NovoAgendamento.PacoteId);
            Debug.WriteLine($"ServicoId: {agendamentoCompleto.ServicoId}");
            agendaVM._populandoCampos = false;
            agendaVM.ForcarAtualizacaoCampos();

            Debug.WriteLine($"NovoCliente: {agendaVM.NovoCliente?.Nome}");
            var view = new EditarAgendamentoView();
            view.DataContext = agendaVM;
            view.FecharSolicitado += (s, e) =>
            {
                MostrarEditarAgendamento = false;
                TelaEditarAgendamento = null;
            };
            TelaEditarAgendamento = view;
            MostrarEditarAgendamento = true;
        }
        public void SelecionarDia(DateTime data)
        {
            var agendamentos = _agendamentoService.GetByDate(data);
            AgendamentosDoDiaSelecionado = new ObservableCollection<Agendamento>(agendamentos);
        }
        partial void OnAgendamentoSelecionadoChanged(Agendamento value)
        {
            if (value != null)
            {
                TipoSelecionado = TipoBusca.Agendamento;
                DetalhesVisiveis = true;
            }
        }

        private void AtualizarCriancasDoCliente(Cliente cliente)
        {
            ListaCriancas.Clear();
            if (cliente is null) return;

            // Busca de banco e adiciona ao ObservableCollection
            var crs = _criancaService.GetByClienteId(cliente.Id);
            foreach (var cr in crs)
                ListaCriancas.Add(cr);
        }
        private void BuscarAgendamento()
        {
            if (int.TryParse(TermoBusca, out int id))
            {
                var agendamento = _agendamentoService.GetById(id);
                AgendamentoSelecionado = agendamento;
                DetalhesVisiveis = agendamento != null;
            }
            else
            {
                // Mensagem: ID inválido
                DetalhesVisiveis = false;
            }
        }
       

        [RelayCommand]
        private void AvancarMes()
        {
            MesAtual = MesAtual.AddMonths(1);
            CarregarDias();
        }

        [RelayCommand]
        private void VoltarMes()
        {
            MesAtual = MesAtual.AddMonths(-1);
            CarregarDias();
        }
        public ObservableCollection<string> DiasSemana { get; set; } = new();
        private void CarregarDias()
        {
            var servicos = _servicoService.GetAll();

            foreach (var servico in servicos)
            {
                switch (servico.Id)
                {
                    case 1: servico.Cor = Brushes.Red; break;
                    case 2: servico.Cor = Brushes.DarkGreen; break;
                    case 3: servico.Cor = Brushes.Orange; break;
                    default: servico.Cor = Brushes.Blue; break;
                }
            }
            DiasSemana.Clear();
            DiasDoMes.Clear();

            string[] diasSemana = { "Dom", "Seg", "Ter", "Qua", "Qui", "Sex", "Sáb" };

            var primeiroDia = new DateTime(MesAtual.Year, MesAtual.Month, 1);
            var ultimoDia = primeiroDia.AddMonths(1).AddDays(-1);
            var diasNoMes = DateTime.DaysInMonth(MesAtual.Year, MesAtual.Month);


            int startIndex = (int)primeiroDia.DayOfWeek;
            var ordemDias = diasSemana.Skip(startIndex).Concat(diasSemana.Take(startIndex)).ToList();
            foreach (var dia in ordemDias)
                DiasSemana.Add(dia);


            var agendamentosDoMes = _agendamentoService.
                GetAll().
                Where(a => a.Data.Date >= primeiroDia && a.Data.Date <= ultimoDia)
                .ToList();

            for (int i = 0; i < diasNoMes; i++)
            {
                var data = primeiroDia.AddDays(i);
                var itens = agendamentosDoMes.Where(a => a.Data.Date == data.Date).ToList();

                var diaVm = new DiaCalendario(data)
                {
                    TemEvento = VerificarSeTemEvento(data),
                    DescricaoEvento = ObterDescricao(data)
                };

                foreach (var ag in itens)
                    diaVm.Agendamentos.Add(ag);

                DiasDoMes.Add(diaVm);
            }
        }

        // Simulações (depois substitua por consultas reais ao banco)
        private bool VerificarSeTemEvento(DateTime data) =>
            data.Day % 3 == 0;

        private string ObterDescricao(DateTime data) =>
            VerificarSeTemEvento(data) ? "Evento Exemplo" : string.Empty;

        private Brush VerCor(DateTime data) =>
            VerificarSeTemEvento(data) ? Brushes.LightGreen : Brushes.Transparent;
    } 

} 
