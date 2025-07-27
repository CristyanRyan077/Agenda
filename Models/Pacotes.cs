using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AgendaNovo.Models
{
    public partial class Pacote : ObservableObject
    {
        public int Id;

        [ObservableProperty] private int? servicoId;
        public Servico? Servico { get; set; }

        [ObservableProperty] private string nome;

        [ObservableProperty] private decimal valor;

    }
}
