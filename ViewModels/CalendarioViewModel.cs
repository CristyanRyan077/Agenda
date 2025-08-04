using AgendaNovo.Interfaces;
using AgendaNovo.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AgendaNovo.ViewModels
{
    public partial class CalendarioViewModel : ObservableObject
    {

        private readonly AgendaViewModel _agenda;
        private readonly IClienteService _clienteService;
        private readonly ICriancaService _criancaService;
        private readonly IAgendamentoService _agendamentoService;
        private readonly IServicoService _servicoService;
        private readonly IPacoteService _pacoteService;


        public CalendarioViewModel(IAgendamentoService agendamentoService,
        IClienteService clienteService,
        ICriancaService criancaService,
        IPacoteService pacoteService,
        IServicoService servicoService)
        {
            _agendamentoService = agendamentoService;
            _clienteService = clienteService;
            _criancaService = criancaService;
            _pacoteService = pacoteService;
            _servicoService = servicoService;
            _clienteService.ClienteInativo();
            MesAtual = DateTime.Today;
            tipoSelecionado = TipoBusca.Cliente;
            CarregarDias();

        }
        public IAgendamentoService AgendamentoService => _agendamentoService;
        public IClienteService ClienteService => _clienteService;
        public ICriancaService CriancaService => _criancaService;
        public IPacoteService PacoteService => _pacoteService;
        public IServicoService ServicoService => _servicoService;
        [ObservableProperty]
        private ObservableCollection<DiaCalendario> diasDoMes = new();
        [ObservableProperty] private ObservableCollection<Agendamento> agendamentosDoDiaSelecionado;

        [ObservableProperty] private ObservableCollection<Cliente> listaClientes = new();


        [ObservableProperty]
        private DateTime mesAtual = DateTime.Today;

        [ObservableProperty] private string termoBusca;
        [ObservableProperty] private TipoBusca tipoSelecionado;
        [ObservableProperty] private Cliente clienteSelecionado;
        [ObservableProperty] private Agendamento agendamentoSelecionado;
        [ObservableProperty] private bool detalhesVisiveis;

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

        private void CarregarDias()
        {
            DiasDoMes.Clear();

            var primeiroDia = new DateTime(MesAtual.Year, MesAtual.Month, 1);
            var ultimoDia = primeiroDia.AddMonths(1).AddDays(-1);
            var diasNoMes = DateTime.DaysInMonth(MesAtual.Year, MesAtual.Month);
            var agendamentosDoMes = _agendamentoService.
                GetAll().
                Where(a => a.Data.Date >= primeiroDia && a.Data.Date <= ultimoDia)
                .ToList();

            for (int i = 0; i < diasNoMes; i++)
            {
                var data = primeiroDia.AddDays(i);
                var agendamentosDoDia = agendamentosDoMes.Where(a => a.Data.Date == data.Date).ToList();

                DiasDoMes.Add(new DiaCalendario
                {
                    Data = data,
                    Agendamentos = new ObservableCollection<Agendamento>(agendamentosDoDia),
                    TemEvento = VerificarSeTemEvento(data),
                    DescricaoEvento = ObterDescricao(data),
                    //CorEvento = VerCor(data)
                });
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
