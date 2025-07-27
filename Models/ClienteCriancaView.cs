using System;
using System.Collections.Generic;
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
        [ObservableProperty] private string? nomeCrianca;
        [ObservableProperty] private Genero genero = Genero.M;
        [ObservableProperty] private int? idade;
        public string? IdadeFormatada =>
        Idade.HasValue ? $"{Idade} {IdadeUnidade}" : null;

        [ObservableProperty] private IdadeUnidade idadeUnidade = IdadeUnidade.Meses;

        [ObservableProperty] private DateOnly? nascimento;

        public DateTime? NascimentoDateTime
        {
            get => Nascimento.HasValue ? Nascimento.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null;
            set
            {
                if (value.HasValue)
                    Nascimento = DateOnly.FromDateTime(value.Value);
                else
                    Nascimento = null;
                OnPropertyChanged(nameof(NascimentoDateTime));
                OnPropertyChanged(nameof(Nascimento));
            }
        }

        partial void OnNascimentoChanged(DateOnly? oldValue, DateOnly? newValue)
        {
            OnPropertyChanged(nameof(Idade));
            OnPropertyChanged(nameof(IdadeUnidade));
        }
    }
}