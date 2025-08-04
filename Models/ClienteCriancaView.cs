using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AgendaNovo.Models
{
    public partial class ClienteCriancaView : ObservableObject
    {
        public int ClienteId { get; set; }
        public int? CriancaId { get; set; }

        [ObservableProperty] private string? nomeCliente;
        [ObservableProperty] private string? telefone;
        [ObservableProperty] private string? email;
        [ObservableProperty] private string? observacao;
        [ObservableProperty] private StatusCliente status = StatusCliente.SA;
        [ObservableProperty]
        private string? facebook;

        [ObservableProperty]
        private string? instagram;
        [ObservableProperty] private string? nomeCrianca;
        [ObservableProperty] private Genero genero = Genero.M;
        [ObservableProperty] private int? idade;
        public decimal TotalPagoMesAtual => Agendamentos
        .Where(a => a.Data.Month == DateTime.Now.Month && a.Data.Year == DateTime.Now.Year)
        .Sum(a => a.ValorPago);
        public decimal TotalPagoHistorico => Agendamentos.Sum(a => a.ValorPago);
        public string? IdadeFormatada =>
        Idade.HasValue ? $"{Idade} {IdadeUnidade}" : null;

        [ObservableProperty] private IdadeUnidade idadeUnidade = IdadeUnidade.Meses;

        [ObservableProperty] private DateOnly? nascimento;

        public List<Agendamento> Agendamentos { get; set; } = new();

        [NotMapped]
        public DateTime? NascimentoDateTime
        {
            get => Nascimento?.ToDateTime(TimeOnly.MinValue);
            set => Nascimento = value.HasValue ? DateOnly.FromDateTime(value.Value) : null;
        }

        partial void OnNascimentoChanged(DateOnly? oldValue, DateOnly? newValue)
        {
            OnPropertyChanged(nameof(Idade));
            OnPropertyChanged(nameof(IdadeUnidade));
        }
    }
}