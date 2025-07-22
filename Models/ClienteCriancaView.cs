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
        public int? ClienteId { get; set; }
        public int? CriancaId { get; set; }

        [ObservableProperty] private string? nomeCliente;
        [ObservableProperty] private string? telefone;
        [ObservableProperty] private string? email;
        [ObservableProperty] private string? nomeCrianca;
        [ObservableProperty] private Genero genero = Genero.M;
        [ObservableProperty] private int? idade;
        [ObservableProperty] private IdadeUnidade idadeUnidade = IdadeUnidade.Anos;
        public string? IdadeFormatada =>
                Idade.HasValue ? $"{Idade} {IdadeUnidade}" : null;

        partial void OnIdadeUnidadeChanged(IdadeUnidade value)
        {
            OnPropertyChanged(nameof(IdadeFormatada));
        }
        partial void OnIdadeChanged(int? value)
        {
            OnPropertyChanged(nameof(IdadeFormatada));
        }
    }
}