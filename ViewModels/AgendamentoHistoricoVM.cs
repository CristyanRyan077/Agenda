using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AgendaNovo.Agendamento;

namespace AgendaNovo.ViewModels
{
    public partial class AgendamentoHistoricoVM : ObservableObject
    {
        public Agendamento? Agendamento { get; set; }
        public int? NumeroMes { get; set; }

        [ObservableProperty]
        private bool estaSendoEditado;
    }
}
