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


        [ObservableProperty] private TimeSpan? horario;

        [ObservableProperty] private string? tema;

        [ObservableProperty] private decimal valor;

        [ObservableProperty] private decimal valorPago;

        [NotMapped]
        public decimal ValorPagoTotal => Pagamentos?.Sum(p => p.Valor) ?? 0m;

        [ObservableProperty]
        private StatusAgendamento status = StatusAgendamento.Pendente;

        [ObservableProperty]
        [Column("FotosReveladas")]
        private FotosReveladas fotos;
        [NotMapped] public int? NumeroMes { get; set; }

        public bool EstaPago => Math.Round(Valor, 2) <= Math.Round(ValorPago, 2);
        public bool Pago { get; set; }

        public ICollection<Pagamento> Pagamentos { get; set; } = new List<Pagamento>();
        public class Pagamento
        {
            public int Id { get; set; }
            public int AgendamentoId { get; set; }
            public Agendamento Agendamento { get; set; } = null!;

            public decimal Valor { get; set; }                // valor da parcela
            public DateTime DataPagamento { get; set; }       // quando pagou
            public string? Metodo { get; set; }               // “PIX”, “Crédito”, etc. (opcional)
            public string? Observacao { get; set; }           // opcional
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        }

        [ObservableProperty] private DateTime data = DateTime.Today;
        partial void OnValorChanged(decimal oldValue, decimal newValue)
        {
            OnPropertyChanged(nameof(EstaPago));

        }
        partial void OnValorPagoChanged(decimal oldValue, decimal newValue)
        {
            OnPropertyChanged(nameof(EstaPago));
            OnPropertyChanged(nameof(valorPago));
        }
    }
}
