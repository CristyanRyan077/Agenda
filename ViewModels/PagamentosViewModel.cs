using AgendaNovo.Interfaces;
using AgendaNovo.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AgendaNovo.Agendamento;

namespace AgendaNovo.ViewModels
{
    public partial class PagamentosViewModel : ObservableObject
    {
        private readonly IPagamentoService _service;

        public PagamentosViewModel(IPagamentoService service, int agendamentoId)
        {
            _service = service;
            AgendamentoId = agendamentoId;
        }

        public int AgendamentoId { get; }

        // Resumo / cabeçalho
        [ObservableProperty] private string? clienteNome;
        [ObservableProperty] private string? servicoNome;
        [ObservableProperty] private DateTime dataAgendamento;
        [ObservableProperty] private decimal valor;
        public decimal ValorPago => Pagamentos.Sum(p => p.Valor);
        public decimal Falta => Math.Max(0, Valor - ValorPago);
        public int PercentualPago => Valor <= 0 ? 0 : (int)Math.Round(Math.Min(ValorPago, Valor) / Valor * 100m);

        // Lista
        [ObservableProperty] private ObservableCollection<PagamentoDto> pagamentos = new();
        [ObservableProperty] private PagamentoDto? pagamentoSelecionado;

        // Novo pagamento (inputs)
        [ObservableProperty] private NovoPagamentoDto novoPagamento = new() { DataPagamento = DateTime.Now };

        // Notificar %/Falta quando ValorPago mudar
  
        partial void OnPagamentosChanged(ObservableCollection<PagamentoDto> value)
        {
            OnPropertyChanged(nameof(ValorPago));
            OnPropertyChanged(nameof(Falta));
            OnPropertyChanged(nameof(PercentualPago));
        }

        [RelayCommand]
        public async Task CarregarAsync()
        {
            var header = await _service.ObterResumoAgendamentoAsync(AgendamentoId);
            ClienteNome = header.ClienteNome;
            ServicoNome = header.ServicoNome;
            DataAgendamento = header.Data;
            Valor = header.Valor;

            var lista = await _service.ListarPagamentosAsync(AgendamentoId);
            Pagamentos = new ObservableCollection<PagamentoDto>(lista);
        }

        [RelayCommand]
        public async Task AdicionarPagamentoAsync()
        {
            if (NovoPagamento.Valor <= 0) return;

            await _service.AdicionarPagamentoAsync(AgendamentoId, new CriarPagamentoDto
            {
                Valor = NovoPagamento.Valor,
                DataPagamento = NovoPagamento.DataPagamento,
                Metodo = NovoPagamento.Metodo,
                Observacao = NovoPagamento.Observacao
            });

            NovoPagamento = new NovoPagamentoDto { DataPagamento = DateTime.Now };
            await CarregarAsync();
        }

        [RelayCommand]
        public async Task EditarPagamentoAsync(PagamentoDto? p)
        {
            if (p is null) return;
            await _service.AtualizarPagamentoAsync(new AtualizarPagamentoDto
            {
                Id = p.Id,
                Valor = p.Valor,
                DataPagamento = p.DataPagamento,
                Metodo = p.Metodo,
                Observacao = p.Observacao
            });
            await CarregarAsync();
        }


        [RelayCommand]
        public async Task ExcluirPagamentoAsync(PagamentoDto? p)
        {
            if (p is null) return;
            await _service.RemoverPagamentoAsync(p.Id);
            await CarregarAsync();
        }

        [RelayCommand] public void ExportarRecibos() { /* opcional */ }
        [RelayCommand] public void Fechar() { /* Window.Close via evento/serviço UI */ }
    }
    public record PagamentoDto(int Id, DateTime DataPagamento, decimal Valor, string? Metodo, string? Observacao);
    public class NovoPagamentoDto { public DateTime DataPagamento { get; set; } public decimal Valor { get; set; } public string? Metodo { get; set; } public string? Observacao { get; set; } }
    public class CriarPagamentoDto : NovoPagamentoDto { }
    public class AtualizarPagamentoDto : NovoPagamentoDto { public int Id { get; set; } }

    public record ResumoAgendamentoDto(string ClienteNome, string ServicoNome, DateTime Data, decimal Valor);
}
