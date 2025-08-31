using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgendaNovo.ViewModels
{
    using AgendaNovo.Interfaces;
    using AgendaNovo.Models;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using ControlzEx.Standard;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    public partial class FinanceiroViewModel : ObservableObject
    {
        private readonly SemaphoreSlim _loadGate = new(1, 1);
        private readonly IAgendamentoService _agendamentoService;

        public FinanceiroViewModel(IAgendamentoService agendamentoService)
        {
            _agendamentoService = agendamentoService;

            // Período padrão = mês atual
            PeriodoInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            PeriodoFim = PeriodoInicio.Value.AddMonths(1).AddDays(-1);
        }
        // 🔹 Filtros
        [ObservableProperty] private DateTime? periodoInicio;
        [ObservableProperty] private DateTime? periodoFim;
        [ObservableProperty] private Servico servicoSelecionado;
        [ObservableProperty] private string statusSelecionado = "Todos";

        // 🔹 KPIs
        [ObservableProperty] private decimal receitaBruta;
        [ObservableProperty] private decimal recebido;
        [ObservableProperty] private decimal emAberto;
        [ObservableProperty] private decimal ticketMedio;
        [ObservableProperty] private int qtdAgendamentos;
        public decimal PercRecebido => ReceitaBruta == 0 ? 0 : Math.Round(Recebido / ReceitaBruta * 100, 2);

        // 🔹 Listas
        [ObservableProperty] private ObservableCollection<RecebivelVM> emAbertoLista = new();
        [ObservableProperty] private ObservableCollection<ServicoResumoVM> servicosResumo = new();

        partial void OnRecebidoChanged(decimal value) => OnPropertyChanged(nameof(PercRecebido));
        partial void OnReceitaBrutaChanged(decimal value) => OnPropertyChanged(nameof(PercRecebido));

        // 🔹 Atalhos de período
        [RelayCommand]
        public void QuickMesAtual()
        {
            PeriodoInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            PeriodoFim = PeriodoInicio.Value.AddMonths(1).AddDays(-1);
            CarregarAsync();
        }

        // 🔹 Método principal
        [RelayCommand]
        public async Task CarregarAsync()
        {

            await _loadGate.WaitAsync();
            try
            {
                var sw = Stopwatch.StartNew();
                var (ini, fim, status, servicoId) = PrepararFiltros();
                var kpis = await _agendamentoService.CalcularKpisAsync(ini, fim, servicoId, status);
                Debug.WriteLine($"KPIs: {sw.Elapsed}");
                var emAberto = await _agendamentoService.ListarEmAbertoAsync(ini, fim, servicoId, status);
                Debug.WriteLine($"EmAberto: {sw.Elapsed}");
                var resumo = await _agendamentoService.ResumoPorServicoAsync(ini, fim, servicoId, status);
                Debug.WriteLine($"Resumo: {sw.Elapsed}");

                ReceitaBruta = kpis.ReceitaBruta;
                Recebido = kpis.Recebido;
                EmAberto = kpis.EmAberto;
                QtdAgendamentos = kpis.QtdAgendamentos;
                TicketMedio = kpis.TicketMedio;

                EmAbertoLista = new ObservableCollection<RecebivelVM>(
                    emAberto.Select(r => new RecebivelVM
                    {
                        Id = r.Id,
                        Data = r.Data,
                        Cliente = r.Cliente,
                        Servico = r.Servico,
                        Valor = r.Valor,
                        ValorPago = r.ValorPago,
                        Falta = r.Valor - Math.Min(r.ValorPago, r.Valor),
                        Status = r.Status
                    }));

                ServicosResumo = new ObservableCollection<ServicoResumoVM>(
                    resumo.Select(s => new ServicoResumoVM
                    {
                        Servico = s.Servico,
                        Receita = s.Receita,
                        Qtd = s.Qtd,
                        TicketMedio = s.TicketMedio
                    }));
            }
            finally
            {
                _loadGate.Release();
            }
        }

        // --- MÉTODOS PRIVADOS ---
        private (DateTime ini, DateTime fim, StatusAgendamento? status, int? servicoId) PrepararFiltros()
        {
            var ini = PeriodoInicio ?? DateTime.MinValue;
            var fim = (PeriodoFim?.Date.AddDays(1).AddTicks(-1)) ?? DateTime.MaxValue;

            StatusAgendamento? status = null;
            if (!string.IsNullOrWhiteSpace(StatusSelecionado) && StatusSelecionado != "Todos" &&
                Enum.TryParse(StatusSelecionado, out StatusAgendamento stParsed))
                status = stParsed;

            return (ini, fim, status, ServicoSelecionado?.Id);
        }


        // 🔹 Exportação
        public void ExportarEmAbertoParaExcel()
        {
            using var wb = new ClosedXML.Excel.XLWorkbook();
            var ws = wb.AddWorksheet("EmAberto");

            // cabeçalho
            string[] headers = { "Data", "Cliente", "Serviço", "Valor", "Pago", "Falta", "Status" };
            for (int i = 0; i < headers.Length; i++)
                ws.Cell(1, i + 1).Value = headers[i];

            // dados
            for (int i = 0; i < EmAbertoLista.Count; i++)
            {
                var r = EmAbertoLista[i];
                ws.Cell(i + 2, 1).Value = r.Data;
                ws.Cell(i + 2, 2).Value = r.Cliente;
                ws.Cell(i + 2, 3).Value = r.Servico;
                ws.Cell(i + 2, 4).Value = r.Valor;
                ws.Cell(i + 2, 5).Value = r.ValorPago;
                ws.Cell(i + 2, 6).Value = r.Falta;
                ws.Cell(i + 2, 7).Value = r.Status;
            }

            ws.Columns().AdjustToContents();
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                $"EmAberto_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");

            wb.SaveAs(path);
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true });
        }
    }

    public class RecebivelVM
    {
        public int Id { get; set; } 
        public DateTime Data { get; set; }
        public string Cliente { get; set; }
        public string Servico { get; set; }
        public decimal Valor { get; set; }
        public decimal ValorPago { get; set; }
        public decimal Falta { get; set; }
        public string Status { get; set; }
        public int PercentualPago => Valor <= 0 ? 0 : (int)Math.Round(Math.Min(ValorPago, Valor) / Valor * 100m);
    }

    public class ServicoResumoVM
    {
        public string Servico { get; set; }
        public decimal Receita { get; set; }
        public int Qtd { get; set; }
        public decimal TicketMedio { get; set; }
    }

}
