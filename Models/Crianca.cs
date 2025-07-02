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
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        public List<Agendamento> Agendamentos { get; set; } = new();
    }
}
