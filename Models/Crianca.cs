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

        [ObservableProperty] private int? idade;

        [ObservableProperty] private string? genero;
<<<<<<< HEAD

        [ObservableProperty] private string idadeUnidade = "anos";
=======
>>>>>>> 339a18d4c7781204c8c80dd673286d34e4ee8714
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        public string IdadeFormatada => $"{Idade} {IdadeUnidade}";
        public List<Agendamento> Agendamentos { get; set; } = new();
    }
}
