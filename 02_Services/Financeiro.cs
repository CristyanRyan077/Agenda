using AgendaNovo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgendaNovo.Services
{
        public sealed class FinanceiroRow
        {
            public int Id { get; set; }                 // Guid? se for
            public DateTime Data { get; set; }
            public int? ServicoId { get; set; }
            public string ServicoNome { get; set; } = "—";
            public string? ClienteNome { get; set; }
            public decimal Valor { get; set; }
            public decimal ValorPago { get; set; }
            public StatusAgendamento Status { get; set; }
        }

        // KPIs consolidados
        public sealed class FinanceiroResumo
        {
            public decimal ReceitaBruta { get; set; }
            public decimal Recebido { get; set; }
            public decimal ReceitaProdutos { get; set; }
            public decimal EmAberto { get; set; }
            public int QtdAgendamentos { get; set; }
            public decimal TicketMedio { get; set; }
            public decimal TicketMedioProdutos { get; set; }
            public int QtdProdutos { get; set; }
            public decimal PercRecebido => ReceitaBruta == 0 ? 0 : Math.Round(Recebido / ReceitaBruta * 100, 2);
        }

        // Tabela de “Em aberto”
        public sealed class RecebivelDTO
        {
            public int Id { get; set; }
            public DateTime Data { get; set; }
            public string? Cliente { get; set; }
            public string Servico { get; set; } = "—";
            public decimal Valor { get; set; }
            public decimal ValorPago { get; set; }
            public decimal Falta => Math.Max(0, Valor - ValorPago);
            public string Status { get; set; } = "";
            public int PercentualPago => Valor <= 0 ? 0 : (int)Math.Round(Math.Min(ValorPago, Valor) / Valor * 100m);
        }

        // Resumo agrupado por serviço
        public sealed class ServicoResumoDTO
        {
            public string Servico { get; set; } = "—";
            public decimal Receita { get; set; }
            public int Qtd { get; set; }
            public decimal TicketMedio { get; set; }
        }
    }
