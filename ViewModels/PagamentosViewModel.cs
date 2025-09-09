using AgendaNovo.Interfaces;
using AgendaNovo.Models;
using AgendaNovo.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.Office2010.Excel;
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
        private readonly IAgendamentoService _agendaservice;
        private readonly IClienteService _clienteservice;

        public PagamentosViewModel(IPagamentoService service, int agendamentoId, IAgendamentoService agendaservice, IClienteService clienteservice)
        {
            _service = service;
            AgendamentoId = agendamentoId;
            _agendaservice = agendaservice;
            _clienteservice = clienteservice;
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

        // editar
        [ObservableProperty] private bool estaEditando;
        [ObservableProperty] private int id;
        [ObservableProperty] private int clienteId;

        [ObservableProperty] private DateTime dataPagamento;


        [ObservableProperty] private string? observacao;
        public decimal ValorPago => Pagamentos.Sum(p => p.Valor);
        public decimal Falta => Math.Max(0, Valor - ValorPago);
        public int PercentualPago => Valor <= 0 ? 0 : (int)Math.Round(Math.Min(ValorPago, Valor) / Valor * 100m);

        // Lista
        [ObservableProperty] private ObservableCollection<PagamentoDto> pagamentos = new();
        [ObservableProperty] private PagamentoDto? pagamentoSelecionado;

        // Novo pagamento (inputs)
        [ObservableProperty] private NovoPagamentoDto novoPagamento = new() { DataPagamento = DateTime.Now };
        public ObservableCollection<MetodoPagamento> Metodo { get; } =
    new ObservableCollection<MetodoPagamento>((MetodoPagamento[])Enum.GetValues(typeof(MetodoPagamento)));


        public MetodoPagamento MetodoSelecionado { get; set; }

        partial void OnPagamentosChanged(ObservableCollection<PagamentoDto> value)
        {
            OnPropertyChanged(nameof(ValorPago));
            OnPropertyChanged(nameof(Falta));
            OnPropertyChanged(nameof(PercentualPago));
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

        partial void OnFotosChanged(FotosReveladas value)
        {
            MostrarBotaoSalvarFotos = true;
        }
        [RelayCommand]
        public async Task SalvarStatusFotosAsync()
        {
            // Atualiza apenas o campo Fotos do agendamento
            await _agendaservice.AtualizarFotosAsync(AgendamentoId, Fotos);

            MostrarBotaoSalvarFotos = false; // botão some após salvar
        }
        [RelayCommand]
        public void SelecionarPagamentoParaEdicao(PagamentoDto? p)
        {
            if (p is null) return;

            Id = p.Id;
            NovoPagamento = new NovoPagamentoDto
            {
                Valor = p.Valor,
                DataPagamento = p.DataPagamento,
                Metodo = p.Metodo,
                Observacao = p.Observacao
            };

            EstaEditando = true; // ativa o modo edição
        }
        private void LimparFormulario()
        {
            Id = 0;
            NovoPagamento = new NovoPagamentoDto { DataPagamento = DateTime.Now };
            EstaEditando = false;
        }

        [RelayCommand]
        public async Task CarregarAsync()
        {
            var header = await _service.ObterResumoAgendamentoAsync(AgendamentoId);
            ClienteId = header.ClienteId;
            ClienteNome = header.ClienteNome;
            ServicoNome = header.ServicoNome;
            DataAgendamento = header.Data;
            Valor = header.Valor;
            Fotos = header.Fotos;

            var lista = await _service.ListarPagamentosAsync(AgendamentoId);
            Pagamentos = new ObservableCollection<PagamentoDto>(lista);
            
        }

        [RelayCommand]
        public async Task SalvarOuAdicionarPagamentoAsync()
        {
            var agendamento = _service.ObterResumoAgendamentoAsync(AgendamentoId);
            if (NovoPagamento.Valor <= 0) return;

            if (EstaEditando)
            {
                // Salvar edição
                var dto = new AtualizarPagamentoDto
                {
                    Id = Id,
                    Valor = NovoPagamento.Valor,
                    DataPagamento = NovoPagamento.DataPagamento,
                    Metodo = NovoPagamento.Metodo,
                    Observacao = NovoPagamento.Observacao
                };
                
                await _service.AtualizarPagamentoAsync(dto);

            }
            else
            {
                // Adicionar novo pagamento
                var dto = new CriarPagamentoDto
                {
                    Valor = NovoPagamento.Valor,
                    DataPagamento = NovoPagamento.DataPagamento,
                    Metodo = NovoPagamento.Metodo,
                    Observacao = NovoPagamento.Observacao
                };
                await _service.AdicionarPagamentoAsync(AgendamentoId, dto);
                
                
            }

            await CarregarAsync();
            await AtualizarStatusAsync();
            LimparFormulario();
        }


        [RelayCommand]
        public async Task ExcluirPagamentoAsync(PagamentoDto? p)
        {
            if (p is null) return;
            await _service.RemoverPagamentoAsync(p.Id);
            await CarregarAsync();
            await AtualizarStatusAsync();
        }

        [RelayCommand] public void ExportarRecibos() { /* opcional */ }
        [RelayCommand] public void Fechar() { /* Window.Close via evento/serviço UI */ }
    }
    public record PagamentoDto(int Id, DateTime DataPagamento, decimal Valor, MetodoPagamento  Metodo, string? Observacao);
    public class NovoPagamentoDto { public DateTime DataPagamento { get; set; } public decimal Valor { get; set; } public MetodoPagamento Metodo { get; set; } public string? Observacao { get; set; } }
    public class CriarPagamentoDto : NovoPagamentoDto { }
    public class AtualizarPagamentoDto : NovoPagamentoDto { public int Id { get; set; } }

    public record ResumoAgendamentoDto(int ClienteId, string ClienteNome, FotosReveladas Fotos, string ServicoNome, DateTime Data, decimal Valor);
}
