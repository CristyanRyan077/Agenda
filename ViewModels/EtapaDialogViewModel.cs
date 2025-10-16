using AgendaNovo.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgendaNovo.ViewModels
{
    public partial class EtapaDialogViewModel : ObservableObject
    {
        public int AgendamentoId { get; set; }
        [ObservableProperty] private EtapaFotos etapa;
        [ObservableProperty] private DateTime dataConclusao = DateTime.Today;
        [ObservableProperty] private string? observacao;
    }
}
