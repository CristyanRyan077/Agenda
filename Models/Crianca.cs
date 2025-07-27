using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AgendaNovo.Models
{
    public partial class Crianca : ObservableObject
    {

        public int Id { get; set; }
        [ObservableProperty] private string? nome;


        [ObservableProperty] private Genero genero = Genero.M;

        [ObservableProperty] private int? idade;

        [ObservableProperty] private IdadeUnidade idadeUnidade = IdadeUnidade.Meses;

        [ObservableProperty] private DateOnly? nascimento;
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        public string? IdadeFormatada => $"{Idade} {IdadeUnidade}";
        public List<Agendamento> Agendamentos { get; set; } = new();
       
    }
}
