using AgendaNovo.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using ControlzEx.Standard;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ValorPago))]
        public ObservableCollection<Pagamento> pagamentos = new();

        [ObservableProperty] private DateTime data = DateTime.Today;

        [ObservableProperty] private TimeSpan? horario;

        [ObservableProperty] private string? tema;

        [ObservableProperty] private decimal valor;

        [ObservableProperty] private StatusAgendamento status = StatusAgendamento.Pendente;

        [ObservableProperty] private FotosReveladas fotos;

        [NotMapped] public decimal ValorPago => Pagamentos?.Sum(p => p.Valor) ?? 0m;
        [NotMapped] public int? NumeroMes { get; set; }

        public int? Mesversario { get; set; }
        public string? MesversarioFormatado => $"Mês {Mesversario}";

        public bool EstaPago => Math.Round(Valor, 2) <= Math.Round(ValorPago, 2);
        public bool Pago { get; set; }


        public partial class Pagamento : ObservableObject
        {
            public int Id { get; set; }
            public int AgendamentoId { get; set; }
            public Agendamento Agendamento { get; set; } = null!;

            public int? AgendamentoProdutoId { get; set; }

            [ObservableProperty] private decimal valor;            // valor da parcela
            [ObservableProperty] private DateTime dataPagamento;   // quando pagou
            [ObservableProperty] private MetodoPagamento metodo;           // “PIX”, “Crédito”, etc. (opcional)
            [ObservableProperty] private string? observacao;       // opcional
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        }
        public class AgendamentoProduto
        {
            public int Id { get; set; }

            public int AgendamentoId { get; set; }
            public Agendamento Agendamento { get; set; } = null!;

            public int ProdutoId { get; set; }
            public Produto Produto { get; set; } = null!;

            public int Quantidade { get; set; } = 1;
            public decimal ValorUnitario { get; set; }
            public decimal ValorTotal => Quantidade * ValorUnitario;
            public DateTime CreatedAt { get; set; } = DateTime.Now;
        }


        partial void OnPagamentosChanged(ObservableCollection<Pagamento> value)
        {
            if (value != null)
                value.CollectionChanged += Pagamentos_CollectionChanged;


            OnPropertyChanged(nameof(ValorPago));
        }
        private void Pagamentos_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(ValorPago));
        }
    }
}
