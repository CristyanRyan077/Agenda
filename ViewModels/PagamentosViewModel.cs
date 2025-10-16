using AgendaNovo.Dtos;
using AgendaNovo.Interfaces;
using AgendaNovo.Models;
using AgendaNovo.Services;
using AgendaNovo.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DocumentFormat.OpenXml.Office2010.Excel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AgendaNovo.Agendamento;
using static AgendaNovo.Services.PagamentoService;

namespace AgendaNovo.ViewModels
{
    public partial class PagamentosViewModel : ObservableObject
    {
        private readonly IPagamentoService _service;
        private readonly IAgendamentoService _agendaservice;
        private readonly IClienteService _clienteservice;
        private readonly IProdutoService _produtoservice;

        public PagamentosViewModel(IPagamentoService service,
            int agendamentoId,
            IAgendamentoService agendaservice,
            IClienteService clienteservice,
            IProdutoService produtoservice)
        {
            _service = service;
            AgendamentoId = agendamentoId;
            _agendaservice = agendaservice;
            _clienteservice = clienteservice;
            _produtoservice = produtoservice;
            LimparFormulario();
    
        }

        public int AgendamentoId { get; }

        // Resumo / cabeçalho
        [ObservableProperty] private string? clienteNome;
        [ObservableProperty] private string? servicoNome;
        [ObservableProperty] private DateTime dataAgendamento;
        [ObservableProperty] private decimal valor;
        [ObservableProperty] private FotosReveladas fotos = FotosReveladas.Pendente;
        [ObservableProperty]
        private bool _mostrarBotaoSalvarFotos = false;
        [ObservableProperty] private ObservableCollection<Produto> produtosDisponiveis = new();

        // editar
        [ObservableProperty] private bool estaEditando;
        [ObservableProperty] private int id;
        [ObservableProperty] private int clienteId;

        [ObservableProperty] private DateTime dataPagamento = DateTime.Today;


        [ObservableProperty] private string? observacao;
        public decimal ValorPago => Historico?.Where(h => h.Tipo == "Pagamento").Sum(h => h.Valor) ?? 0m;
        public decimal Falta => Math.Max(0, Valor - ValorPago);
        public int PercentualPago => Valor <= 0 ? 0 : (int)Math.Round(Math.Min(ValorPago, Valor) / Valor * 100m);

        // Lista
        [ObservableProperty] private HistoricoFinanceiroDto? itemSelecionado;
        [ObservableProperty] private ObservableCollection<HistoricoFinanceiroDto> pagamentos = new();

        // Novo pagamento (inputs)
        public ObservableCollection<MetodoPagamento> Metodo { get; } =
        new ObservableCollection<MetodoPagamento>((MetodoPagamento[])Enum.GetValues(typeof(MetodoPagamento)));

        [ObservableProperty] private ObservableCollection<HistoricoFinanceiroDto> historico = new();
        [ObservableProperty] private CriarPagamentoDto novoPagamento = new();
        [ObservableProperty] private CriarProdutoAgendamentoDto novoProduto = new();
        [ObservableProperty] private bool modoProduto;
        [ObservableProperty] private Produto? produtoSelecionado;


        [ObservableProperty]
        private TipoLancamento tipoLancamento = TipoLancamento.Pagamento;
        public string TextoBotaoPrimario =>
            TipoLancamento == TipoLancamento.Pagamento ? "Adicionar Pagamento" : "Adicionar Produto";

        public MetodoPagamento MetodoSelecionado { get; set; }

        partial void OnHistoricoChanged(ObservableCollection<HistoricoFinanceiroDto> value)
        {
            OnPropertyChanged(nameof(ValorPago));
            OnPropertyChanged(nameof(Falta));
            OnPropertyChanged(nameof(PercentualPago));
        }
        partial void OnTipoLancamentoChanged(TipoLancamento value)
        {
            // Mantém compatibilidade com a sua lógica já existente
            ModoProduto = (value == TipoLancamento.Produto);
            OnPropertyChanged(nameof(TextoBotaoPrimario));
        }

        public async Task AtualizarStatusAsync()
        {
            if (ValorPago >= Valor)
            {
                // Total pago, marcar como concluído
                _clienteservice.AtivarSePendente(clienteId);
                _agendaservice.AtivarSePendente(AgendamentoId);
            }
            else
            {
                // Ainda falta valor, deixar como pendente
                _clienteservice.ValorIncompleto(clienteId);
                _agendaservice.ValorIncompleto(AgendamentoId);
            }
        }
        public void CarregarProdutos()
        {
            var lista =  _produtoservice.GetAll(); // crie a interface/serviço
            ProdutosDisponiveis = new ObservableCollection<Produto>(lista);
        }

        partial void OnFotosChanged(FotosReveladas value)
        {
            MostrarBotaoSalvarFotos = true;
        }
        [RelayCommand]
        public void SelecionarHistoricoParaEdicao(HistoricoFinanceiroDto? item)
        {
            if (item == null) return;

            ItemSelecionado = item;

            if (item.Tipo == "Pagamento")
            {
                NovoPagamento = new CriarPagamentoDto
                {
                    Valor = item.Valor,
                    DataPagamento = item.Data,
                    Metodo = item.Metodo ?? MetodoPagamento.Pix, // valor default se null
                    Observacao = item.Descricao // usa Descricao no lugar de Observacao
                };
                ModoProduto = false;
            }
            else if (item.Tipo == "Produto")
            {
                NovoProduto = new CriarProdutoAgendamentoDto
                {
                    ProdutoId = item.Id,
                    Quantidade = 1, // ou mapear se armazenou
                    ValorUnitario = item.Valor
                };
                ModoProduto = true;
            }

            EstaEditando = true;
        }
        private void LimparFormulario()
        {
            Id = 0;
            NovoPagamento = new CriarPagamentoDto { DataPagamento = DateTime.Today };
            NovoProduto = new CriarProdutoAgendamentoDto();
            ProdutoSelecionado = null;
            EstaEditando = false;
            ModoProduto = (TipoLancamento == TipoLancamento.Produto);
        }


        [RelayCommand]
        public async Task CarregarAsync()
        {
            try
            {
                var header = await _service.ObterResumoAgendamentoAsync(AgendamentoId);
                ClienteId = header.ClienteId;
                ClienteNome = header.ClienteNome;
                ServicoNome = header.ServicoNome;
                DataAgendamento = header.Data;
                Valor = header.Valor;
                Fotos = header.Fotos;

                var lista = await _service.ListarHistoricoAsync(AgendamentoId);
                Historico = new ObservableCollection<HistoricoFinanceiroDto>(lista);
                CarregarProdutos();
                OnPropertyChanged(nameof(ValorPago));
                OnPropertyChanged(nameof(Falta));
                OnPropertyChanged(nameof(PercentualPago));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro no CarregarAsync: {ex}");
                throw;
            }
        }
        private void NotificarFinanceiro()
        {
            WeakReferenceMessenger.Default.Send(
                new DadosAtualizadosMessage(clienteId: ClienteId, agendamentoId: AgendamentoId)
            );
        }

        [RelayCommand]
        public async Task SalvarOuAdicionarHistoricoAsync()
        {
            if (!ModoProduto)
            {
                // === PAGAMENTO ===
                if (NovoPagamento.Valor <= 0) return;

                if (EstaEditando && ItemSelecionado?.Tipo == "Pagamento")
                {
                    var dto = new AtualizarPagamentoDto
                    {
                        Id = ItemSelecionado.Id,
                        Valor = NovoPagamento.Valor,
                        DataPagamento = NovoPagamento.DataPagamento,
                        Metodo = NovoPagamento.Metodo,
                        Observacao = NovoPagamento.Observacao
                    };

                    await _service.AtualizarPagamentoAsync(dto);
                }
                else
                {
                    var dto = new CriarPagamentoDto
                    {
                        Valor = NovoPagamento.Valor,
                        DataPagamento = NovoPagamento.DataPagamento,
                        Metodo = NovoPagamento.Metodo,
                        Observacao = NovoPagamento.Observacao
                    };

                    await _service.AdicionarPagamentoAsync(AgendamentoId, dto);
                   

                }
            }

            await CarregarAsync();
            await AtualizarStatusAsync();
            LimparFormulario();
            NotificarFinanceiro();
        }
        [RelayCommand]
        public async Task AdicionarProdutoAsync()
        {
            if (NovoProduto.ProdutoId <= 0 || NovoProduto.Quantidade <= 0) return;

            await _service.AdicionarProdutoAoAgendamentoAsync(
                AgendamentoId,
                NovoProduto,
                metodo: MetodoPagamento.Pix 
            );
            await CarregarAsync();
            await AtualizarStatusAsync();
            LimparFormulario();
            NotificarFinanceiro();
        }

        partial void OnProdutoSelecionadoChanged(Produto? p)
        {
            if (p is null)
            {
                NovoProduto = new CriarProdutoAgendamentoDto();
                return;
            }

            NovoProduto = new CriarProdutoAgendamentoDto
            {
                ProdutoId = p.Id,
                Quantidade = NovoProduto?.Quantidade > 0 ? NovoProduto.Quantidade : 1,
                ValorUnitario = (!EstaEditando || NovoProduto?.ValorUnitario <= 0) ? p.Valor : NovoProduto!.ValorUnitario
            };
        }


        [RelayCommand]
        public async Task ExcluirPagamentoAsync(HistoricoFinanceiroDto? h)
        {
            if (h is null) return;

            if (h.Tipo == "Pagamento")
                await _service.RemoverPagamentoAsync(h.Id);
            else if (h.Tipo == "Produto")
                await _service.RemoverProdutoDoAgendamentoAsync(h.Id);

            await CarregarAsync();
            await AtualizarStatusAsync();
            NotificarFinanceiro();
        }
        [RelayCommand]
        public void DefinirValorRapido(decimal valor)
        {
            if (valor <= 0) return;
            NovoPagamento ??= new();
            NovoPagamento.Valor = valor;
        }
        [RelayCommand]
        public void DefinirQuantidadeRapida(int quantidade)
        {
            if (quantidade <= 0) return;
            NovoProduto ??= new();
            NovoProduto.Quantidade = quantidade;
        }

        [RelayCommand] public void ExportarRecibos() { /* opcional */ }
        [RelayCommand] public void Fechar() { /* Window.Close via evento/serviço UI */ }
    }
    public partial class NovoPagamentoDto : ObservableObject
    {
        [ObservableProperty] private DateTime dataPagamento = DateTime.Today;
        [ObservableProperty] private decimal valor;
        [ObservableProperty] private MetodoPagamento metodo;
        [ObservableProperty] private string? observacao;
    }
    public class CriarPagamentoDto : NovoPagamentoDto { }
    public partial class CriarProdutoAgendamentoDto : ObservableObject
    {
        [ObservableProperty] private int produtoId;
        [ObservableProperty] private int quantidade = 1;
        [ObservableProperty] private decimal valorUnitario;
    }
    public class AtualizarPagamentoDto : NovoPagamentoDto { public int Id { get; set; } }
   

    public record ResumoAgendamentoDto(int ClienteId, string ClienteNome, FotosReveladas Fotos, string ServicoNome, DateTime Data, decimal Valor);
}
