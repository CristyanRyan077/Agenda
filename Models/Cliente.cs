using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgendaNovo.Models;
using CommunityToolkit.Mvvm.ComponentModel;


namespace AgendaNovo.Models
{

    public partial class Cliente : ObservableObject
    {
        public int Id { get; set; }

        [ObservableProperty] private string? nome;
        public string NomeComId => $"{Nome} (ID: {Id})";
        [ObservableProperty] private string? telefone;
        [ObservableProperty] private string? email;
        [ObservableProperty] private string? observacao = string.Empty;
        [ObservableProperty] private StatusCliente status = StatusCliente.Pendente;
        [ObservableProperty]
        private string? facebook;

        [ObservableProperty]
        private string? instagram;

        public List<Agendamento> Agendamentos { get; set; } = new();
        public List<Crianca> Criancas { get; set; } = new();
    }
}
