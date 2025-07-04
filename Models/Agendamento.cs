using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgendaNovo.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AgendaNovo
{
    public partial class Agendamento : ObservableObject
    {
        public int Id { get; set; }

        public int ClienteId { get; set; }
        public Cliente ?Cliente { get; set; }
        public int CriancaId { get; set; }
        public Crianca? Crianca { get; set; }

        public string ?Servico { get; set; }

        [ObservableProperty] private string? pacote;
        public string ?Horario { get; set; }
        public string ?Tema { get; set; }

        [ObservableProperty] private decimal valor;
<<<<<<< HEAD

        [ObservableProperty] private decimal valorPendente;

        public bool EstaPago => Math.Round(Valor, 2) == Math.Round(ValorPendente, 2);

=======
        public decimal ValorPendente { get; set; }
>>>>>>> 339a18d4c7781204c8c80dd673286d34e4ee8714
        public DateTime Data { get; set; } = DateTime.Today;
    }
}
