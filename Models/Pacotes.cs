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
        [ObservableProperty]
        private int id;

        [ObservableProperty]
        private string ?nome;

        [ObservableProperty]
        private string ?categoria; 

        [ObservableProperty]
        private decimal valor;
    }
}
