using AgendaNovo.Converters;
using AgendaNovo.Helpers;
using AgendaNovo.Interfaces;
using AgendaNovo.Models;
using AgendaNovo.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlzEx.Standard;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgendaNovo.ViewModels
{
    public partial class FotosViewModel : ObservableObject
    {
        private readonly IAgendamentoService _agendamentoService;
        private readonly IClienteService _clienteService;
        private readonly ICriancaService _criancaService;
        private readonly IPacoteService _pacoteService;
        private readonly IServicoService _servicoService;

        // 🔹 Filtros
        [ObservableProperty] private DateTime? periodoInicio;
        [ObservableProperty] private DateTime? periodoFim;
        [ObservableProperty] private Servico servicoSelecionado;
        [ObservableProperty] private Produto produtoSelecionado;
        [ObservableProperty] private string statusSelecionado = "Todos";
        [ObservableProperty] private string? filtroClienteNome;
        [ObservableProperty] private ObservableCollection<FotoProcessoVM> fotosResumo = new();
        [ObservableProperty] private ObservableCollection<Cliente> listaClientes = new();
        [ObservableProperty]
        private bool mostrarSugestoes = false;
        [ObservableProperty]
        private string nomeDigitado = string.Empty;
        public ObservableCollection<Cliente> ClientesFiltrados { get; set; } = new();

        public FotosViewModel(IAgendamentoService agendamentoService,
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
            PeriodoInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            PeriodoFim = PeriodoInicio.Value.AddMonths(1).AddDays(-1);
        }
        [RelayCommand]
        public async Task CarregarAsync()
        {
            var (ini, fim, status, servicoId, produtoId, clienteNome) = PrepararFiltros();
            var fotos = await _agendamentoService.ListarProcessoFotosAsync(ini, fim, status, clienteNome);

            FotosResumo = new ObservableCollection<FotoProcessoVM>(fotos);
            Debug.WriteLine("VM carregado!");
        }
        private (DateTime ini, DateTime fim, EtapaStatus? status, int? servicoId, int? produtoId, string? clienteNome) PrepararFiltros()
        {
            var ini = PeriodoInicio ?? DateTime.MinValue;
            var fim = (PeriodoFim?.Date.AddDays(1).AddTicks(-1)) ?? DateTime.MaxValue;

            EtapaStatus? status = null;
            if (!string.IsNullOrWhiteSpace(StatusSelecionado) && StatusSelecionado != "Todos" &&
                Enum.TryParse(StatusSelecionado, out EtapaStatus stParsed))
                status = stParsed;
            var clienteNome = string.IsNullOrWhiteSpace(FiltroClienteNome) ? null : FiltroClienteNome!.Trim();

            return (ini, fim, status, ServicoSelecionado?.Id, ProdutoSelecionado?.Id, clienteNome);
        }
        // 🔹 Atalhos de período
        [RelayCommand]
        public void QuickMesAtual()
        {
            PeriodoInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            PeriodoFim = PeriodoInicio.Value.AddMonths(1).AddDays(-1);
            CarregarAsync();
        }
        [RelayCommand]
        private void LimparFiltroCliente()
        {
            FiltroClienteNome = null;
        }
        [RelayCommand]
        private void AbrirEtapaPelaLinha(FotoProcessoVM linha)
        {
            if (linha is null) return;

            // carrega o agendamento para passar ao serviço
            var ag = _agendamentoService.GetById(linha.AgendamentoId);
            if (ag is null) return;

            
            var etapaSugerida = EtapaHelper.GetProximaEtapa(ag); 


            var ok = _agendamentoService.AbrirEtapaDialog(new SetEtapaParam
            {
                Agendamento = ag,
                Etapa = etapaSugerida.Value
            });

            if (ok)
            {
                // recarrega o grid para refletir a nova etapa
                _ = CarregarAsync();
            }
        }
        partial void OnNomeDigitadoChanged(string value)
        {
            var termo = value?.ToLower() ?? "";
            var filtrados = ListaClientes
                .Where(c =>
            (!string.IsNullOrEmpty(c.Nome) && c.Nome.ToLower().Contains(termo)) ||
            (!string.IsNullOrEmpty(c.Telefone) && c.Telefone.EndsWith(termo)))
                .ToList();

            ClientesFiltrados.Clear();
            foreach (var cliente in filtrados)
                ClientesFiltrados.Add(cliente);

            MostrarSugestoes = ClientesFiltrados.Any();
        }
        [RelayCommand]
        private async Task ExportarWordAsync()
        {
            var sfd = new SaveFileDialog
            {
                Filter = "Documento do Word (*.docx)|*.docx",
                FileName = $"EntregaFotos_{DateTime.Now:yyyyMMdd}.docx"
            };
            if (sfd.ShowDialog() == true)
            {
                var caminho = sfd.FileName;
                // Se preferir não travar a UI:
                await Task.Run(() => WordExportHelper.GenerateFotosResumoDocx(FotosResumo, caminho));
            }
        }
    }


   
    public class FotoProcessoVM
    {
        public int AgendamentoId { get; set; }
        public DateTime Data { get; set; }
        public string Cliente { get; set; } = "";
        public string Crianca { get; set; } = "";
        public string Telefone { get; set; } = "";
        public int? Mesversario { get; set; }
        public string? MesversarioFormatado { get; set; }
        public string Servico { get; set; } = "";
        public string EtapaAtual { get; set; } = ""; // ex.: "Tratamento"
        public DateTime? EscolhaData { get; set; }
        public DateTime? TratamentoData { get; set; }
        public DateTime? RevelarData { get; set; }
        public DateTime? EntregaData { get; set; }
        public EtapaStatus Status { get; set; } // Pendente/Hoje/Atrasado/Concluido
    }

}


