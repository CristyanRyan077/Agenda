using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgendaNovo.ViewModels
{
    using AgendaNovo.Interfaces;
    using AgendaNovo.Models;
    using ClosedXML.Excel;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using ControlzEx.Standard;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Win32;
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
        [ObservableProperty] private ExportTipo exportSelecionado = ExportTipo.Ambos;

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
        [RelayCommand]
        public void ExportarParaExcel()
        {
            static void FormatBRL(IXLRange range)
            {
                // moeda brasileira, 2 casas, separador 1.234,56 em QUALQUER Excel
                range.Style.NumberFormat.Format = "[$-pt-BR]R$ #,##0.00";
            }
            static void FormatBR2(IXLRange range)
            {
                // número com 2 casas, 1.234,56
                range.Style.NumberFormat.Format = "[$-pt-BR]#,##0.00";
            }
            using var wb = new ClosedXML.Excel.XLWorkbook();
            var ini = PeriodoInicio?.Date ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var fim = PeriodoFim?.Date ?? ini.AddMonths(1).AddDays(-1);
            string periodoLabel = $"{ini:dd/MM/yyyy} a {fim:dd/MM/yyyy}";

            // === Aba Em Aberto ===
            if (exportSelecionado == ExportTipo.EmAberto || exportSelecionado == ExportTipo.Ambos)
            {
                var ws = wb.AddWorksheet("EmAberto");

                // Título e período (sem InsertRowsAbove)
                ws.Cell(1, 1).Value = "Cobranças (Em Aberto)";
                ws.Range(1, 1, 1, 7).Merge().Style.Font.SetBold().Font.SetFontSize(14);
                ws.Cell(2, 1).Value = $"Período: {periodoLabel}";
                ws.Range(2, 1, 2, 7).Merge().Style.Font.SetItalic();

                // Cabeçalho começa na linha 4
                var headers = new[] { "Data", "Cliente", "Serviço", "Valor", "Pago", "Falta", "Status" };
                int headerRow = 4;
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = ws.Cell(headerRow, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                }

                // Dados a partir da linha 5
                int row = headerRow + 1;
                foreach (var r in EmAbertoLista)
                {
                    ws.Cell(row, 1).Value = r.Data;
                    ws.Cell(row, 1).Style.DateFormat.Format = "dd/MM/yyyy";

                    ws.Cell(row, 2).Value = r.Cliente;
                    ws.Cell(row, 3).Value = r.Servico;

                    ws.Cell(row, 4).Value = (double)r.Valor;
                    ws.Cell(row, 5).Value = (double)r.ValorPago;
                    ws.Cell(row, 6).Value = (double)r.Falta;

                    ws.Cell(row, 4).Style.NumberFormat.Format = "R$ #.##0,00";
                    ws.Cell(row, 5).Style.NumberFormat.Format = "R$ #.##0,00";
                    ws.Cell(row, 6).Style.NumberFormat.Format = "R$ #.##0,00";

                    ws.Cell(row, 7).Value = r.Status;

                    row++;
                }
                ws.Column(1).Style.DateFormat.Format = "dd/MM/yyyy";
                FormatBRL(ws.Range(headerRow + 1, 4, row - 1, 6));

                ws.Columns().AdjustToContents();
                ws.Column(2).Width = Math.Max(ws.Column(2).Width, 18); // Cliente
                ws.Column(4).Width = Math.Max(ws.Column(4).Width, 14); // Valor
                ws.Column(5).Width = Math.Max(ws.Column(5).Width, 14); // Pago
                ws.Column(6).Width = Math.Max(ws.Column(6).Width, 14); // Falta
                ws.SheetView.FreezeRows(headerRow); // congela até o cabeçalho
            }

            // === Aba Resumo ===
            if (exportSelecionado == ExportTipo.Resumo || exportSelecionado == ExportTipo.Ambos)
            {
                var ws = wb.AddWorksheet("Resumo");

                ws.Cell(1, 1).Value = "Resumo Financeiro";
                ws.Range(1, 1, 1, 4).Merge().Style.Font.SetBold().Font.SetFontSize(14);
                ws.Cell(2, 1).Value = $"Período: {periodoLabel}";
                ws.Range(2, 1, 2, 4).Merge().Style.Font.SetItalic();

                // KPIs
                ws.Cell(4, 1).Value = "Receita Bruta";
                ws.Cell(5, 1).Value = "Recebido";
                ws.Cell(6, 1).Value = "Em Aberto";
                ws.Cell(7, 1).Value = "Qtd. Agendamentos";
                ws.Cell(8, 1).Value = "Ticket Médio";
                ws.Range(4, 1, 8, 1).Style.Font.Bold = true;
 
     ;

                ws.Cell(4, 2).Value = (double)ReceitaBruta;
                ws.Cell(5, 2).Value = (double)Recebido;
                ws.Cell(6, 2).Value = (double)EmAberto;
                ws.Cell(7, 2).Value = QtdAgendamentos;
                ws.Cell(8, 2).Value = (double)TicketMedio;

                FormatBRL(ws.Range(4, 2, 6, 2));
                ws.Cell(7, 2).Style.NumberFormat.Format = "0";
                FormatBRL(ws.Cell(8, 2).AsRange());
                ws.Columns(1, 4).AdjustToContents();
                ws.Column(2).Width = Math.Max(ws.Column(2).Width, 16);
                ws.Column(4).Width = Math.Max(ws.Column(4).Width, 16);


                // Por serviço
                int start = 10;
                ws.Cell(start, 1).Value = "Por Serviço";
                ws.Cell(start + 1, 1).Value = "Serviço";
                ws.Cell(start + 1, 2).Value = "Receita";
                ws.Cell(start + 1, 3).Value = "Qtd";
                ws.Cell(start + 1, 4).Value = "Ticket Médio";
                ws.Range(start + 1, 1, start + 1, 4).Style.Font.Bold = true;
                ws.Range(start + 1, 1, start + 1, 4).Style.Fill.BackgroundColor = XLColor.LightGray;

                int r = start + 2;
                foreach (var s in ServicosResumo)
                {
                    ws.Cell(r, 1).Value = s.Servico;
                    ws.Cell(r, 2).Value = (double)s.Receita;
                    ws.Cell(r, 3).Value = s.Qtd;
                    ws.Cell(r, 4).Value = (double)s.TicketMedio;

                    ws.Cell(r, 2).Style.NumberFormat.Format = "R$ #.##0,00";
                    ws.Cell(r, 4).Style.NumberFormat.Format = "R$ #.##0,00";
                    r++;
                }
                int firstDataRow = start + 2;
                int lastDataRow = r - 1;
                if (lastDataRow >= firstDataRow)
                {
                    FormatBRL(ws.Range(firstDataRow, 2, lastDataRow, 2)); // Receita
                    ws.Range(firstDataRow, 3, lastDataRow, 3).Style.NumberFormat.Format = "0"; // Qtd
                    FormatBRL(ws.Range(firstDataRow, 4, lastDataRow, 4)); // Ticket Médio
                }

                // Totais (usando fórmulas A1)
                ws.Cell(r, 1).Value = "TOTAL";
                ws.Cell(r, 1).Style.Font.Bold = true;
                ws.Cell(r, 2).FormulaA1 = $"SUM(B{start + 2}:B{r - 1})";
                ws.Cell(r, 3).FormulaA1 = $"SUM(C{start + 2}:C{r - 1})";
                ws.Cell(r, 4).FormulaA1 = $"IF(C{r}>0, B{r}/C{r}, 0)";
                ws.Cell(r, 2).Style.NumberFormat.Format = "R$ #.##0,00";
                ws.Cell(r, 4).Style.NumberFormat.Format = "R$ #.##0,00";

                ws.Range(start + 1, 1, r, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Range(start + 1, 1, r, 4).Style.Border.InsideBorder = XLBorderStyleValues.Dotted;
                FormatBRL(ws.Cell(r, 2).AsRange());
                ws.Cell(r, 3).Style.NumberFormat.Format = "0";
                FormatBRL(ws.Cell(r, 4).AsRange());

                ws.Columns().AdjustToContents();
            }

            var salvar = new SaveFileDialog
            {
                FileName = $"Financeiro_{(exportSelecionado == ExportTipo.EmAberto ? "EmAberto" : exportSelecionado == ExportTipo.Resumo ? "Resumo" : "Completo")}_{ini:yyyyMMdd}-{fim:yyyyMMdd}.xlsx",
                Filter = "Arquivo Excel (*.xlsx)|*.xlsx"
            };
            if (salvar.ShowDialog() != true) return;

            wb.SaveAs(salvar.FileName);
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
