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
        [ObservableProperty] private ObservableCollection<Agendamento> agendamentosFiltrados = new();
        [ObservableProperty] private string textoPesquisa = string.Empty;
        [ObservableProperty] private DateTime? dataSelecionada;
        public ObservableCollection<Agendamento> ListaAgendamentos { get; } = new();
        public IRelayCommand<(Agendamento ag, DateTime novaData)> MoverAgendamentoCommand { get; }
        public ObservableCollection<Crianca> ListaCriancas { get; } = new();
        [ObservableProperty] private ObservableCollection<Cliente> listaClientes = new();

        [ObservableProperty] private object telaEditarAgendamento;
        [ObservableProperty] private bool mostrarEditarAgendamento;


        [ObservableProperty]
        private DateTime mesAtual = DateTime.Today;
        [ObservableProperty] private string filtroSelecionado;
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
        private void FecharEdicao()
        {
            MostrarEditarAgendamento = false;
            TelaEditarAgendamento = null;
            var agendaVM = AgendaViewModel;
            agendaVM.NovoAgendamento = new Agendamento();
            agendaVM.NovoCliente = new Cliente();
            agendaVM.ClienteSelecionado = null;
            agendaVM.CriancaSelecionada = null;
            agendaVM.ItemSelecionado = null;
            agendaVM.HorarioTexto = string.Empty;
            agendaVM._populandoCampos = false;
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


            if (agendamentoCompleto.Servico != null)
            {
                agendaVM.ServicoSelecionado = agendaVM.ListaServicos
                    .FirstOrDefault(s => s.Id == agendamentoCompleto.Servico.Id);
            }
            agendaVM.Pacoteselecionado = agendaVM.ListaPacotesFiltrada.FirstOrDefault(p => p.Id == agendamentoCompleto.Pacote?.Id);
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
        partial void OnFiltroSelecionadoChanged(string value)
        {
            FiltrarAgendamentos();
        }
        public void SelecionarDia(DateTime data)
        {
            DataSelecionada = data;
        }
        partial void OnDataSelecionadaChanged(DateTime? value)
        {
            FiltrarAgendamentos();
        }
        private void FiltrarAgendamentos()
        {
            IEnumerable<Agendamento> filtrado = ListaAgendamentos;
            var agora = DateTime.Now;

            if (FiltroSelecionado == "Pendente")
            {
                filtrado = filtrado.Where(a => a.Status == StatusAgendamento.Pendente);
            }
            else if (FiltroSelecionado == "Concluido")
            {
                filtrado = filtrado.Where(a => a.Status == StatusAgendamento.Concluido);
            }

            // filtro por texto
            if (!string.IsNullOrWhiteSpace(TextoPesquisa))
            {
                filtrado = filtrado.Where(a => a.Cliente.Nome.Contains(TextoPesquisa, StringComparison.OrdinalIgnoreCase));
            }

            // filtro por data
            if (DataSelecionada.HasValue)
            {
                filtrado = filtrado.Where(a => a.Data == DataSelecionada.Value.Date);
            }
           

            AgendamentosFiltrados = new ObservableCollection<Agendamento>(filtrado);
        }
        partial void OnTextoPesquisaChanged(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                DataSelecionada = null;
                FiltroSelecionado = "Todos";
            }
 
            FiltrarAgendamentos();
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
        private bool VerificarSeTemEvento(DateTime data) =>
            data.Day % 3 == 0;

        private string ObterDescricao(DateTime data) =>
            VerificarSeTemEvento(data) ? "Evento Exemplo" : string.Empty;

        private Brush VerCor(DateTime data) =>
            VerificarSeTemEvento(data) ? Brushes.LightGreen : Brushes.Transparent;
    } 

} 
