using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AgendaNovo.Models
{
    public partial class Servico : ObservableObject
    {
        public int Id { get; set; }
        [ObservableProperty] private string? nome;

        [ObservableProperty] private bool? possuiCrianca;
    }
}
