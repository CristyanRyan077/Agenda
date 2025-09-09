using AgendaNovo.Controles;
using AgendaNovo.Interfaces;
using AgendaNovo.Migrations;
using AgendaNovo.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using static AgendaNovo.Agendamento;
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
        private readonly IPagamentoService _pagamentoService;


        public CalendarioViewModel(AgendaViewModel agendaViewModel, IAgendamentoService agendamentoService,
        IClienteService clienteService,
        ICriancaService criancaService,
        IPacoteService pacoteService,
        IServicoService servicoService,
        IPagamentoService pagamentoService)
        {

            AgendaViewModel = agendaViewModel;
            _agendamentoService = agendamentoService;
            _clienteService = clienteService;
            _criancaService = criancaService;
            _pacoteService = pacoteService;
            _servicoService = servicoService;
            _pagamentoService = pagamentoService;
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
            WeakReferenceMessenger.Default.Register<DadosAtualizadosMessage>(this, (r, m) =>
            {

                agendaViewModel.OnDadosAtualizados(m);
                RefreshCalendar();

            });
        }

        public IAgendamentoService AgendamentoService => _agendamentoService;
        public IClienteService ClienteService => _clienteService;
        public ICriancaService CriancaService => _criancaService;
        public IPacoteService PacoteService => _pacoteService;
        public IServicoService ServicoService => _servicoService;

        public IPagamentoService PagamentoService => _pagamentoService;

        public IEnumerable<ServicoLegenda> GlossarioServicos => ServicoPalette.All;
        [ObservableProperty]
        private ObservableCollection<DiaCalendario> diasDoMes = new();
        [ObservableProperty] private ObservableCollection<Agendamento> agendamentosDoDiaSelecionado;
        [ObservableProperty] private ObservableCollection<Agendamento> agendamentosFiltrados = new();
        [ObservableProperty] private ObservableCollection<AgendamentoHistoricoVM> historicoAgendamentos = new();
        [ObservableProperty]
        private int? agendamentoEditandoId;
        [ObservableProperty] private bool completouAcompanhamento;
        [ObservableProperty] private string textoPesquisa = string.Empty;
        [ObservableProperty] private DateTime? dataSelecionada;
        [ObservableProperty] private int? agendamentoSelecionadoIdFiltro;
        public ObservableCollection<Agendamento> ListaAgendamentos { get; } = new();
        public IRelayCommand<(Agendamento ag, DateTime novaData)> MoverAgendamentoCommand { get; }
        public ObservableCollection<Crianca> ListaCriancas { get; } = new();
        [ObservableProperty] private ObservableCollection<Cliente> listaClientes = new();

        [ObservableProperty] private object telaEditarAgendamento;
        [ObservableProperty] private bool mostrarEditarAgendamento;
        [ObservableProperty] private object telaHistoricoCliente;
        [ObservableProperty] private object? pagamentosView;
        [ObservableProperty] private bool mostrarHistoricoCliente;
        [ObservableProperty] private bool mostrarPagamentos;
        [ObservableProperty] private PagamentosViewModel? pagamentosVM;


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
        public void SelecionarDia(DateTime data)
        {
            AgendamentoSelecionadoIdFiltro = null; // limpamos filtro por agendamento
            DataSelecionada = data.Date;           // marca o dia
            foreach (var d in DiasDoMes)
                d.Selecionado = d.Data.Date == DataSelecionada.Value.Date;
            Debug.WriteLine($"SelecionarDia: {data:dd/MM/yyyy}");
        }

        public void SelecionarAgendamento(Agendamento ag)
        {
            AgendamentoSelecionadoIdFiltro = ag?.Id;
            DataSelecionada = ag?.Data.Date;       // ainda marca o dia do card
            foreach (var d in DiasDoMes)
                d.Selecionado = DataSelecionada.HasValue && d.Data.Date == DataSelecionada.Value.Date;
            Debug.WriteLine($"SelecionarAgendamento: {ag?.Id}");
        }
        public void RefreshAgendamento(Agendamento agendamentoAtualizado)
        {
            var existenteFiltrado = AgendamentosFiltrados.FirstOrDefault(a => a.Id == agendamentoAtualizado.Id);
            if (existenteFiltrado != null)
            {
                var indexF = AgendamentosFiltrados.IndexOf(existenteFiltrado);
                AgendamentosFiltrados[indexF] = agendamentoAtualizado;
            }

        }
        partial void OnPagamentosVMChanged(PagamentosViewModel? value)
    => System.Diagnostics.Debug.WriteLine($"PagamentosVM set: {value != null}");
        partial void OnMostrarPagamentosChanged(bool value)
        {
            System.Diagnostics.Debug.WriteLine($"📌 MostrarPagamentos mudou para: {value}");
        }
        [RelayCommand]
        public async Task AbrirPagamentosAsync(int agendamentoId)
        {
            System.Diagnostics.Debug.WriteLine($"🔵 Abrindo pagamentos para agendamento {agendamentoId}");
            PagamentosVM = new PagamentosViewModel(_pagamentoService, agendamentoId, _agendamentoService, _clienteService);
            await PagamentosVM.CarregarAsync();
            MostrarPagamentos = true;
            System.Diagnostics.Debug.WriteLine($"✅ MostrarPagamentos = {MostrarPagamentos}");
        }
        [RelayCommand]
        public void FecharPagamentos()
        {
            MostrarPagamentos = false;           
            PagamentosView = null;
        }

        [RelayCommand]
        private void FecharEdicao()
        {
            mostrarPagamentos = false;
            mostrarHistoricoCliente = false;
            var agendaVM = AgendaViewModel;
            agendaVM.NovoAgendamento = new Agendamento();
            agendaVM.NovoCliente = new Cliente();
            agendaVM.ClienteSelecionado = null;
            agendaVM.CriancaSelecionada = null;
            agendaVM.ItemSelecionado = null;
            agendaVM.HorarioTexto = string.Empty;
        }

        partial void OnClienteSelecionadoChanged(Cliente value)
        {
            AtualizarCriancasDoCliente(value);
        }
        public bool TemHistorico => HistoricoAgendamentos?.Any() == true;
        public void HistoricoCliente()
        {
            var agendamentosdocliente = _clienteService.GetAgendamentos(AgendamentoSelecionado.ClienteId) ?? new List<Agendamento>();

            var acompanhamentos = agendamentosdocliente
                
                .Where(a => a.Servico.Nome == "Acompanhamento Mensal")
                .OrderBy(a => a.Data)
                .Select((a, index) => new { a.Id, NumeroMes = index + 1 })
                .ToList();

            var historico = agendamentosdocliente
                
                .OrderByDescending(a => a.Data)
                .Select(a => new AgendamentoHistoricoVM
                {
                    Agendamento = a,
                    NumeroMes = acompanhamentos.FirstOrDefault(x => x.Id == a.Id)?.NumeroMes
                })
                .ToList();


            HistoricoAgendamentos = new ObservableCollection<AgendamentoHistoricoVM>(historico);
            OnPropertyChanged(nameof(TemHistorico));// Ordena do mais recente para o mais antigo
            AplicarDestaqueNoHistorico();
            // Se quiser calcular acompanhamento mensal completo:
            var mensalcompleto = agendamentosdocliente
                .Where(a => a.Status == StatusAgendamento.Concluido
                            && a.Data.Year == DateTime.Now.Year
                            && a.ServicoId == 2)
                .Select(a => a.Data.Month)
                .Distinct();


            CompletouAcompanhamento = mensalcompleto.Count() == 12;
            var view = new HistoricoUsuario();
            view.DataContext = this;
            TelaHistoricoCliente = view;
            MostrarHistoricoCliente = true;
        }
        [RelayCommand]
        private void EditarPeloHistorico(AgendamentoHistoricoVM item)
        {
            if (item?.Agendamento == null) return;
            EditarAgendamentoPorId(item.Agendamento.Id);
        }
        public void AplicarDestaqueNoHistorico()
        {
            if (HistoricoAgendamentos == null) return;
            foreach (var h in HistoricoAgendamentos)
                h.EstaSendoEditado = (AgendamentoEditandoId.HasValue &&
                                      h.Agendamento?.Id == AgendamentoEditandoId.Value);
        }
        public void EditarAgendamentoPorId(int id)
        {
            var agendamentoCompleto = _agendamentoService.GetById(id);
            if (agendamentoCompleto == null) return;

            AgendamentoEditandoId = agendamentoCompleto.Id;

            var agendaVM = AgendaViewModel;
            agendaVM._populandoCampos = true;

            ListaCriancas.Clear();
            int? agendamentoIdNotificacao = agendamentoCompleto.Id;
            int? clienteIdNotificacao = agendamentoCompleto.Id;

            agendaVM.NovoAgendamento = agendamentoCompleto;
            agendaVM.NovoCliente = agendamentoCompleto.Cliente;
            agendaVM.DataSelecionada = agendamentoCompleto.Data;
            agendaVM.ClienteSelecionado = agendamentoCompleto.Cliente;
            agendaVM.CriancaSelecionada = agendamentoCompleto.Crianca;
            agendaVM.Fotosreveladas = agendamentoCompleto.Fotos;
            agendaVM.ItemSelecionado = agendamentoCompleto;
            agendaVM.NovoPagamento = new Pagamento { Valor = agendamentoCompleto.Pagamentos.Sum(p => p.Valor) };

            agendaVM.HorarioTexto = agendamentoCompleto.Horario?.ToString(@"hh\:mm");
            agendaVM.CarregarServicos();
            agendaVM.CarregarPacotes();
            if (agendamentoCompleto.ServicoId.HasValue)
                agendaVM.FiltrarPacotesPorServico(agendamentoCompleto.ServicoId.Value);
            agendaVM.PreencherValorPacoteSelecionado(agendaVM.NovoAgendamento.PacoteId);

            if (agendamentoCompleto.Servico != null)
                agendaVM.ServicoSelecionado = agendaVM.ListaServicos.FirstOrDefault(s => s.Id == agendamentoCompleto.Servico.Id);

            agendaVM.Pacoteselecionado = agendaVM.ListaPacotesFiltrada
                .FirstOrDefault(p => p.Id == agendamentoCompleto.Pacote?.Id);

            agendaVM._populandoCampos = false;
            agendaVM.ForcarAtualizacaoCampos();
            if (agendamentoIdNotificacao.HasValue || clienteIdNotificacao.HasValue)
                WeakReferenceMessenger.Default.Send(
                    new DadosAtualizadosMessage(
                        clienteId: clienteIdNotificacao, agendamentoId: agendamentoIdNotificacao
                    )
                );

            // cria/mostra o modal se ainda não estiver aberto:
            if (!MostrarEditarAgendamento || TelaEditarAgendamento is null)
            {
                var view = new EditarAgendamentoView { DataContext = agendaVM };
                view.FecharSolicitado += (s, e) =>
                {
                  


                    MostrarEditarAgendamento = false;
                    TelaEditarAgendamento = null;
                    AgendamentoEditandoId = null;
                    AplicarDestaqueNoHistorico();
                };

                TelaEditarAgendamento = view;
                MostrarEditarAgendamento = true;
            }

            // atualiza destaque nos cards
            AplicarDestaqueNoHistorico();
        }
        partial void OnFiltroSelecionadoChanged(string value)
        {
            AgendamentoSelecionadoIdFiltro = null;
            dataSelecionada = dataSelecionada;
            FiltrarAgendamentos();
            
        }
        partial void OnDataSelecionadaChanged(DateTime? value)
        {
            if (value.HasValue) AgendamentoSelecionadoIdFiltro = null;
            FiltrarAgendamentos();
            FiltroSelecionado = "Todos";
        }
        private void FiltrarAgendamentos()
        {
            IEnumerable<Agendamento> filtrado = ListaAgendamentos;
            // prioridade 1: filtro por agendamento específico
            if (AgendamentoSelecionadoIdFiltro.HasValue)
                filtrado = filtrado.Where(a => a.Id == AgendamentoSelecionadoIdFiltro.Value);
            // prioridade 2: filtro por dia
            else if (DataSelecionada.HasValue)
                filtrado = filtrado.Where(a => a.Data.Date == DataSelecionada.Value.Date);

            if (FiltroSelecionado == "Pendente") filtrado = filtrado.Where(a => a.Status == StatusAgendamento.Pendente);
            else if (FiltroSelecionado == "Concluido") filtrado = filtrado.Where(a => a.Status == StatusAgendamento.Concluido);
            else if (FiltroSelecionado == "Revelado") filtrado = filtrado.Where(a => a.Fotos == FotosReveladas.Revelado);
            else if (FiltroSelecionado == "Entregue") filtrado = filtrado.Where(a => a.Fotos == FotosReveladas.Entregue);


            if (!string.IsNullOrWhiteSpace(TextoPesquisa))
            {
                DataSelecionada = null;
                AgendamentoSelecionadoIdFiltro = null;
                filtrado = filtrado.Where(a => a.Cliente.Nome.Contains(TextoPesquisa, StringComparison.OrdinalIgnoreCase));
            }

            AgendamentosFiltrados = new ObservableCollection<Agendamento>(filtrado);
            Debug.WriteLine($"FiltrarAgendamentos: DataSel={DataSelecionada}, AgSel={AgendamentoSelecionadoIdFiltro}");
        }
        partial void OnAgendamentoSelecionadoIdFiltroChanged(int? value)
        {
            // Se marcou um card específico, garanta que o dia correspondente siga marcado
            if (value.HasValue)
            {
                var ag = ListaAgendamentos.FirstOrDefault(a => a.Id == value.Value);
                if (ag != null)
                {
                    DataSelecionada = ag.Data.Date;
                    foreach (var d in DiasDoMes)
                        d.Selecionado = d.Data.Date == DataSelecionada.Value.Date;
                }
            }
            FiltrarAgendamentos(); // 👈 ESSENCIAL
        }
        partial void OnTextoPesquisaChanged(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                DataSelecionada = null;
                FiltroSelecionado = "Todos";
                AgendamentoSelecionadoIdFiltro = null;
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
                if (DataSelecionada.HasValue)
                    foreach (var d in DiasDoMes)
                        d.Selecionado = d.Data.Date == DataSelecionada.Value.Date;
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
