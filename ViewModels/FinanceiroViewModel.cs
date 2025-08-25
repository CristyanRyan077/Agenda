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
    using Microsoft.EntityFrameworkCore;
    using System.Collections.ObjectModel;
    using System.Linq;

    public partial class FinanceiroViewModel : ObservableObject
    {
        private readonly SemaphoreSlim _loadGate = new(1, 1);
        private readonly AgendaContext _db;
        public AgendaViewModel AgendaViewModel { get; }
        private readonly AgendaViewModel _agenda;
        private readonly IClienteService _clienteService;
        private readonly ICriancaService _criancaService;
        private readonly IAgendamentoService _agendamentoService;
        private readonly IServicoService _servicoService;
        private readonly IPacoteService _pacoteService;
        public FinanceiroViewModel(AgendaContext db, IAgendamentoService agendamentoService,
        IClienteService clienteService,
        ICriancaService criancaService,
        IPacoteService pacoteService,
        IServicoService servicoService)
        {
            _db = db;
            _agendamentoService = agendamentoService;
            _clienteService = clienteService;
            _criancaService = criancaService;
            _pacoteService = pacoteService;
            _servicoService = servicoService;

            PeriodoInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            PeriodoFim = PeriodoInicio.Value.AddMonths(1).AddDays(-1);

        }

        [ObservableProperty] private DateTime? periodoInicio;
        [ObservableProperty] private DateTime? periodoFim;
        [ObservableProperty] private Servico servicoSelecionado;
        [ObservableProperty] private string statusSelecionado; // "Todos", "Pendente", "Concluido", "Cancelado"

        // KPIs
        [ObservableProperty] private decimal receitaBruta;
        [ObservableProperty] private decimal recebido;
        [ObservableProperty] private decimal emAberto;
        public decimal PercRecebido => ReceitaBruta == 0 ? 0 : Math.Round(Recebido / ReceitaBruta * 100, 2);
        [ObservableProperty] private decimal ticketMedio;
        [ObservableProperty] private int qtdAgendamentos;

        // Tabelas
        public ObservableCollection<RecebivelVM> EmAbertoLista { get; } = new();
        public ObservableCollection<ServicoResumoVM> ServicosResumo { get; } = new();

        [RelayCommand]
        public void QuickMesAtual()
        {
            PeriodoInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            PeriodoFim = PeriodoInicio.Value.AddMonths(1).AddDays(-1);
            CarregarAsync();
        }

        [RelayCommand]
        public async Task CarregarAsync()
        {
            System.Diagnostics.Debug.WriteLine($"[VM] Carregar START tid={Environment.CurrentManagedThreadId}");
            await _loadGate.WaitAsync();
            try
            {

                var ini = PeriodoInicio ?? DateTime.MinValue;
                var fim = (PeriodoFim?.Date.AddDays(1).AddTicks(-1)) ?? DateTime.MaxValue;

                StatusAgendamento? st = null;
                if (!string.IsNullOrWhiteSpace(StatusSelecionado) && StatusSelecionado != "Todos" &&
                    Enum.TryParse(StatusSelecionado, out StatusAgendamento stParsed))
                    st = stParsed;

                var servId = ServicoSelecionado?.Id;

                // KPIs
                var kpis = await _agendamentoService.CalcularKpisAsync(ini, fim, servId, st);
                ReceitaBruta = kpis.ReceitaBruta;
                Recebido = kpis.Recebido;
                EmAberto = kpis.EmAberto;
                QtdAgendamentos = kpis.QtdAgendamentos;
                TicketMedio = kpis.TicketMedio;
                OnPropertyChanged(nameof(PercRecebido));

                // Em aberto
                EmAbertoLista.Clear();
                var emAberto = await _agendamentoService.ListarEmAbertoAsync(ini, fim, servId, st);
                foreach (var r in emAberto)
                    EmAbertoLista.Add(new RecebivelVM
                    {
                        Id = r.Id,
                        Data = r.Data,
                        Cliente = r.Cliente,
                        Servico = r.Servico,
                        Valor = r.Valor,
                        ValorPago = r.ValorPago,
                        Status = r.Status
                    });

                // Resumo por serviço
                ServicosResumo.Clear();
                var resumo = await _agendamentoService.ResumoPorServicoAsync(ini, fim, servId, st);
                foreach (var s in resumo)
                    ServicosResumo.Add(new ServicoResumoVM { Servico = s.Servico, Receita = s.Receita, Qtd = s.Qtd, TicketMedio = s.TicketMedio });
            }
            finally
            {
                System.Diagnostics.Debug.WriteLine($"[VM] Carregar END");
            }


        }
        public void ExportarEmAbertoParaExcel()
        {
            // usa ClosedXML (você já tem no projeto)
            using var wb = new ClosedXML.Excel.XLWorkbook();
            var ws = wb.AddWorksheet("EmAberto");
            ws.Cell(1, 1).Value = "Data";
            ws.Cell(1, 2).Value = "Cliente";
            ws.Cell(1, 3).Value = "Serviço";
            ws.Cell(1, 4).Value = "Valor";
            ws.Cell(1, 5).Value = "Pago";
            ws.Cell(1, 6).Value = "Falta";
            ws.Cell(1, 7).Value = "Status";

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
            var path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"EmAberto_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
            wb.SaveAs(path);
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true });
        }
    }

    public class RecebivelVM
    {
        public int Id { get; set; } // Guid se for o caso
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
