using AgendaNovo._01_Interfaces;
using AgendaNovo.Controles;
using AgendaNovo.Interfaces;
using AgendaNovo.Migrations;
using AgendaNovo.Models;
using AgendaNovo.Views;
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using static AgendaNovo.Agendamento;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AgendaNovo.ViewModels
{
    public partial class CalendarioViewModel : ObservableRecipient, IRecipient<DadosAtualizadosMessage>
    {
        public AgendaViewModel AgendaViewModel { get; }
        private readonly AgendaViewModel _agenda;
        private readonly IClienteService _clienteService;
        private readonly ICriancaService _criancaService;
        private readonly IAgendamentoService _agendamentoService;
        private readonly IServicoService _servicoService;
        private readonly IPacoteService _pacoteService;
        private readonly IPagamentoService _pagamentoService;
        private readonly IProdutoService _produtoService;
        private readonly IAcoesService _acoes;
        public IAsyncRelayCommand<Agendamento> EditarAgendamentoCommand { get; }
        public IAsyncRelayCommand<Agendamento> PagamentosAgendamentoCommand { get; }


        public CalendarioViewModel(
        AgendaViewModel agendaViewModel,
        IAgendamentoService agendamentoService,
        IClienteService clienteService,
        ICriancaService criancaService,
        IPacoteService pacoteService,
        IServicoService servicoService,
        IPagamentoService pagamentoService,
        IProdutoService produtoService,
        IAcoesService acoesService)
        {
            IsActive = true;
            AgendaViewModel = agendaViewModel;
            _agendamentoService = agendamentoService;
            _clienteService = clienteService;
            _criancaService = criancaService;
            _pacoteService = pacoteService;
            _servicoService = servicoService;
            _pagamentoService = pagamentoService;
            _produtoService = produtoService;
            _acoes = acoesService;
            _clienteService.ClienteInativo();
            
            MesAtual = DateTime.Today;
            tipoSelecionado = TipoBusca.Cliente;
            CarregarDias();
            MoverAgendamentoAsyncCommand =
            new AsyncRelayCommand<(Agendamento ag, DateTime novaData)>(ReagendarAsync);
            RefreshCalendar();

            EditarAgendamentoCommand =
                new AsyncRelayCommand<Agendamento>(EditarAgendamentoAsync);
            PagamentosAgendamentoCommand =
                new AsyncRelayCommand<Agendamento>(AbrirPagamentosAsync);




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
        public IAsyncRelayCommand<(Agendamento ag, DateTime novaData)> MoverAgendamentoAsyncCommand { get; }
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

        public void Receive(DadosAtualizadosMessage m)
        {
            AgendaViewModel.OnDadosAtualizados(m);
            RefreshCalendar();

            if (MostrarHistoricoCliente && m.ClienteId == AgendamentoSelecionado?.ClienteId)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    HistoricoCliente(m.ClienteId.Value);
                    AplicarDestaqueNoHistorico();
                });
            }
        }
        private readonly List<TimeSpan> _horariosFixos = new()
        {
            TimeSpan.Parse("09:00"),
            TimeSpan.Parse("10:00"),
            TimeSpan.Parse("11:00"),
            TimeSpan.Parse("14:00"),
            TimeSpan.Parse("15:00"),
            TimeSpan.Parse("16:00"),
            TimeSpan.Parse("17:00"),
            TimeSpan.Parse("18:00"),
            TimeSpan.Parse("19:00")
        };
        private async Task ReagendarAsync((Agendamento ag, DateTime novaData) p)
        {
            var (ag, novaData) = p;
            if (ag is null) return;
            var horarioEscolhido = ag.Horario;

            if (HorarioOcupado(novaData, ag.Horario, ag.Id))
            {
                // pega sugestões
                var sugestoes = SugerirProximosHorariosLivres(novaData, ag.Horario, 6);

                if (sugestoes.Count == 0)
                {
                    MessageBox.Show(
                        $"O dia {novaData:dd/MM} está lotado nos horários fixos.\n" +
                        $"Tente outro dia ou ajuste manualmente.",
                        "Conflito de Horário", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var picker = new SelecionarHorarioWindow(sugestoes, ag.Horario);
                var ok = picker.ShowDialog() == true;
                if (!ok) return; // cancelado

                horarioEscolhido = picker.HorarioSelecionado;
            }
            var velhaData = ag.Data; var velhoHorario = ag.Horario;
            ag.Horario = horarioEscolhido;
            MoverAgendamentoInMemory(ag, novaData);


            try
            {
                // 2) Persiste no banco
                await _agendamentoService.ReagendarAsync(ag.Id, novaData, horarioEscolhido);

                // 3) Opcional: avisar outras telas/VMs
                WeakReferenceMessenger.Default.Send(
                    new DadosAtualizadosMessage(clienteId: ag.ClienteId, agendamentoId: ag.Id)
                );


                RefreshCalendar();
            }
            catch (Exception ex)
            {
                // Reversão simples se a persistência falhar
                MoverAgendamentoInMemory(ag, ag.Data); // volta (ou guarde a velhaData antes)
                Debug.WriteLine($"Erro ao reagendar: {ex}");
                // Mostre um toast/MessageBox se preferir
            }
        }
        
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
            AgendamentoSelecionadoIdFiltro = null; 
            DataSelecionada = data.Date;          
            if (DataSelecionada.HasValue)
            {
                foreach (var d in DiasDoMes)
                    d.Selecionado = d.Data.Date == DataSelecionada.Value.Date;
                Debug.WriteLine($"SelecionarDia: {data:dd/MM/yyyy}");
            }
        }

        public void SelecionarAgendamento(Agendamento ag)
        {
            AgendamentoSelecionado = ag;
            AgendamentoSelecionadoIdFiltro = ag?.Id;
            DataSelecionada = ag?.Data.Date;     
            if (DataSelecionada.HasValue)
            {
                foreach (var d in DiasDoMes)
                    d.Selecionado = DataSelecionada.HasValue && d.Data.Date == DataSelecionada.Value.Date;
            }
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
        public async Task AbrirPagamentosAsync(Agendamento ag)
        {
            if (ag is null) return;

            SelecionarAgendamento(ag);

            PagamentosVM = await _acoes.CriarPagamentosViewModelAsync(ag.Id);
            MostrarPagamentos = true;

            HistoricoAgendamentos = new ObservableCollection<AgendamentoHistoricoVM>(
                await _acoes.ObterHistoricoClienteAsync(ag.ClienteId));
            OnPropertyChanged(nameof(TemHistorico));
            AplicarDestaqueNoHistorico();
            var view = new HistoricoUsuario { DataContext = this };
            TelaHistoricoCliente = view;
            MostrarHistoricoCliente = true;
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
            MostrarEditarAgendamento = false;
            MostrarHistoricoCliente = false;
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
        public void HistoricoCliente(int clienteId)
        {
            var agendamentosdocliente = _clienteService.GetAgendamentos(clienteId) ?? new List<Agendamento>();


            var historico = agendamentosdocliente              
                .OrderByDescending(a => a.Data)
                .Select(a => new AgendamentoHistoricoVM
                {
                    Agendamento = a,
                    NumeroMes = a.Mesversario
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
        private async Task EditarAgendamentoAsync(Agendamento ag)
        {
            if (ag is null) return;

            // 1) Seleção/estado de UI 
            SelecionarAgendamento(ag);

            // 2) Pede dados ao serviço (sem tocar em UI dentro do serviço)
            var dto = await _acoes.PrepararEdicaoAsync(ag.Id);
            if (dto is null) return;

            // 3) Preenche sua AgendaViewModel com os dados retornados
            var agendaVM = AgendaViewModel;
            agendaVM._populandoCampos = true;

            // monta objetos “editáveis” sem navegação, como você já fazia:
            agendaVM.NovoAgendamento = new Agendamento
            {
                Id = dto.Agendamento.Id,
                Data = dto.Agendamento.Data,
                Horario = dto.Agendamento.Horario,
                Tema = dto.Agendamento.Tema,
                Valor = dto.Agendamento.Valor,
                ServicoId = dto.Agendamento.ServicoId,
                PacoteId = dto.Agendamento.PacoteId,
                CriancaId = dto.Agendamento.CriancaId,
                Fotos = dto.Agendamento.Fotos,
                Mesversario = dto.Agendamento.Mesversario
            };

            agendaVM.NovoCliente = new Cliente
            {
                Id = dto.Cliente?.Id ?? 0,
                Nome = dto.Cliente?.Nome,
                Telefone = dto.Cliente?.Telefone,
                Observacao = dto.Cliente?.Observacao
            };

            // listas pré-carregadas
            agendaVM.ListaServicos = new ObservableCollection<Servico>(dto.Servicos);
            agendaVM.ListaPacotes = new ObservableCollection<Pacote>(dto.Pacotes);
            agendaVM.ListaPacotesFiltrada = new ObservableCollection<Pacote>(dto.PacotesFiltrados);

            // seleções auxiliares
            if (dto.Agendamento.ServicoId.HasValue)
                agendaVM.ServicoSelecionado = agendaVM.ListaServicos
                    .FirstOrDefault(s => s.Id == dto.Agendamento.ServicoId.Value);

            agendaVM.Pacoteselecionado = agendaVM.ListaPacotesFiltrada
                .FirstOrDefault(p => p.Id == dto.Agendamento.PacoteId);

            agendaVM._populandoCampos = false;
            agendaVM.ForcarAtualizacaoCampos();

            // 4) UI: abrir modal, destacar histórico etc.
            HistoricoAgendamentos = new ObservableCollection<AgendamentoHistoricoVM>(
                await _acoes.ObterHistoricoClienteAsync(ag.ClienteId));
            OnPropertyChanged(nameof(TemHistorico));
            AplicarDestaqueNoHistorico();

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
        }

        public void EditarAgendamentoPorId(int id)
        {
            var a = _agendamentoService.GetByIdAsNoTracking(id);
            if (a == null) return;

            AgendamentoEditandoId = a.Id;

            var agendaVM = AgendaViewModel;
            agendaVM._populandoCampos = true;

            ListaCriancas.Clear();
            int? agendamentoIdNotificacao = a.Id;
            int? clienteIdNotificacao = a.ClienteId;

            agendaVM.NovoAgendamento = new Agendamento
            {
                Id = a.Id,
                Data = a.Data,
                Horario = a.Horario,
                Tema = a.Tema,
                Valor = a.Valor,
                ServicoId = a.ServicoId,
                PacoteId = a.PacoteId,
                CriancaId = a.CriancaId,
                Fotos = a.Fotos,
                Mesversario = a.Mesversario
                // não setar navegações aqui
            };

            agendaVM.NovoCliente = new Cliente
            {
                Id = a.ClienteId,
                Nome = a.Cliente?.Nome,
                Telefone = a.Cliente?.Telefone,
                Observacao = a.Cliente?.Observacao
                // sem coleções/navegações
            };
            agendaVM.CarregarServicos();
            agendaVM.CarregarPacotes();
            if (a.ServicoId.HasValue)
                agendaVM.FiltrarPacotesPorServico(a.ServicoId.Value);
            agendaVM.PreencherValorPacoteSelecionado(agendaVM.NovoAgendamento.PacoteId);

            if (a.Servico != null)
                agendaVM.ServicoSelecionado = agendaVM.ListaServicos.FirstOrDefault(s => s.Id == a.Servico.Id);

            agendaVM.Pacoteselecionado = agendaVM.ListaPacotesFiltrada
                .FirstOrDefault(p => p.Id == a.Pacote?.Id);

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
            var agendamentoIdFiltroLocal = AgendamentoSelecionadoIdFiltro;
            var dataSelecionadaLocal = DataSelecionada;
            var filtroLocal = FiltroSelecionado;
            var textoLocal = TextoPesquisa;

            IEnumerable<Agendamento> filtrado = ListaAgendamentos;
            // prioridade 1: filtro por agendamento específico
            if (agendamentoIdFiltroLocal.HasValue)
                filtrado = filtrado.Where(a => a.Id == agendamentoIdFiltroLocal.Value);
            // prioridade 2: filtro por dia
            else if (dataSelecionadaLocal.HasValue)
                filtrado = filtrado.Where(a => a.Data.Date == dataSelecionadaLocal.Value.Date);

            // filtros por status / fotos
            if (filtroLocal == "Pendente") filtrado = filtrado.Where(a => a.Status == StatusAgendamento.Pendente);
            else if (filtroLocal == "Concluido") filtrado = filtrado.Where(a => a.Status == StatusAgendamento.Concluido);
            else if (filtroLocal == "Revelado") filtrado = filtrado.Where(a => a.Fotos == FotosReveladas.Revelado);
            else if (filtroLocal == "Entregue") filtrado = filtrado.Where(a => a.Fotos == FotosReveladas.Entregue);

            // busca por texto
            if (!string.IsNullOrWhiteSpace(textoLocal))
            {
                // evita NullReference caso Cliente ou Nome sejam null
                filtrado = filtrado.Where(a => !string.IsNullOrEmpty(a.Cliente?.Nome) &&
                    a.Cliente.Nome.Contains(textoLocal, StringComparison.OrdinalIgnoreCase));
            }

            // força avaliação aqui e atualiza a coleção
            AgendamentosFiltrados = new ObservableCollection<Agendamento>(filtrado.ToList());
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
                        d.Selecionado = DataSelecionada.HasValue && d.Data.Date == DataSelecionada.Value.Date;
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
        private void MoverAgendamentoInMemory(Agendamento ag, DateTime novaData)
        {
            if (ag == null) return;
            var velhaData = ag.Data.Date;
            var novaDataDate = novaData.Date;

            if (velhaData == novaDataDate) return;

            // 1) Atualiza o modelo (e dispara PropertyChanged)
            ag.Data = novaDataDate; 

            // 2) Tira do dia antigo
            var diaAntigo = DiasDoMes.FirstOrDefault(d => d.Data.Date == velhaData);
            diaAntigo?.Agendamentos.Remove(ag);

            // 3) Coloca no novo dia (em ordem)
            var diaNovo = DiasDoMes.FirstOrDefault(d => d.Data.Date == novaDataDate);
            if (diaNovo != null)
            {
                // insere ordenado por horário
                int idx = 0;
                while (idx < diaNovo.Agendamentos.Count && diaNovo.Agendamentos[idx].Horario <= ag.Horario) idx++;
                diaNovo.Agendamentos.Insert(idx, ag);
            }

            // 4) Mantém lista “plana” sincronizada (se você usa)
            var idxPlano = ListaAgendamentos.IndexOf(ag);
            if (idxPlano >= 0)
            {
                // Se Agendamento implementa INotifyPropertyChanged corretamente, isso já basta.
                // Se não, force o CollectionChanged:
                ListaAgendamentos.RemoveAt(idxPlano);
                // re-inserir mantendo ordenação global se você exibe em alguma grid:
                int pos = 0;
                while (pos < ListaAgendamentos.Count &&
                       (ListaAgendamentos[pos].Data < ag.Data ||
                        (ListaAgendamentos[pos].Data == ag.Data && ListaAgendamentos[pos].Horario <= ag.Horario)))
                    pos++;
                ListaAgendamentos.Insert(pos, ag);
            }

            // 5) Ajusta seleção/destaques se você usa highlight por dia
            DataSelecionada = novaDataDate;
            foreach (var d in DiasDoMes)
                d.Selecionado = d.Data.Date == novaDataDate;

            // Se seu filtro depende de DataSelecionada/AgendamentoSelecionadoIdFiltro:
            FiltrarAgendamentos();
        }
        private bool HorarioOcupado(DateTime dia, TimeSpan? horario, int agendamentoIdIgnorar = 0)
        {
            var vmDia = DiasDoMes.FirstOrDefault(d => d.Data.Date == dia.Date);
            if (vmDia is null) return false;
            return vmDia.Agendamentos.Any(a => a.Horario == horario && a.Id != agendamentoIdIgnorar);
        }
        private List<TimeSpan> HorariosLivres(DateTime dia)
        {
            var vmDia = DiasDoMes.FirstOrDefault(d => d.Data.Date == dia.Date);

            if (vmDia is null) return _horariosFixos.ToList();

            var ocupados = vmDia.Agendamentos
            .Select(a => a.Horario.Value)
            .Where(h => h != null)
            .ToHashSet();

            var livres = _horariosFixos
            .Where(h => !ocupados.Contains(h))
            .ToList();

            return livres;
        }
        private List<TimeSpan> SugerirProximosHorariosLivres(DateTime dia, TimeSpan? aPartir, int max = 4)
        {
            var livres = HorariosLivres(dia).OrderBy(h => h).ToList();
            var depois = livres.Where(h => h >= aPartir).ToList();
            var antes = livres.Where(h => h < aPartir).ToList();
            var result = new List<TimeSpan>();
            result.AddRange(depois);
            result.AddRange(antes);
            return result.Take(max).ToList();
        }
    } 

} 
