using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgendaNovo.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using ControlzEx.Standard;

namespace AgendaNovo
{
    public partial class Agendamento : ObservableObject
    {

        public int Id { get; set; }

        public int ClienteId { get; set; }
        public Cliente ?Cliente { get; set; }   
        public int? CriancaId { get; set; }
        public Crianca? Crianca { get; set; }
        
        public Servico? Servico { get; set; }
        public int? ServicoId { get; set; }
        public Pacote? Pacote { get; set; }
        public int? PacoteId { get; set; }

        public ICollection<Pagamento> Pagamentos { get; set; } = new List<Pagamento>();

        [ObservableProperty] private DateTime data = DateTime.Today;

        [ObservableProperty] private TimeSpan? horario;

        [ObservableProperty] private string? tema;

        [ObservableProperty] private decimal valor;

        //[ObservableProperty] private decimal valorPagoLegacy;

        [ObservableProperty] private StatusAgendamento status = StatusAgendamento.Pendente;

        [ObservableProperty] private FotosReveladas fotos;

        [NotMapped] public decimal ValorPago => Pagamentos?.Sum(p => p.Valor) ?? 0m;
        [NotMapped] public int? NumeroMes { get; set; }

        public bool EstaPago => Math.Round(Valor, 2) <= Math.Round(ValorPago, 2);
        public bool Pago { get; set; }


        public partial class Pagamento : ObservableObject
        {
            public int Id { get; set; }
            public int AgendamentoId { get; set; }
            public Agendamento Agendamento { get; set; } = null!;

            [ObservableProperty] private decimal valor;            // valor da parcela
            [ObservableProperty] private DateTime dataPagamento;   // quando pagou
            [ObservableProperty] private string? metodo;           // “PIX”, “Crédito”, etc. (opcional)
            [ObservableProperty] private string? observacao;       // opcional
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        }


        partial void OnValorChanged(decimal oldValue, decimal newValue)
        {
            OnPropertyChanged(nameof(EstaPago));

        }
    }
}
