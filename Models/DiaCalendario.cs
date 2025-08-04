using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AgendaNovo.Models
{
    public class DiaCalendario
    {
        public DateTime Data { get; set; }
        public bool TemEvento { get; set; }
        public string? DescricaoEvento { get; set; }
        public ObservableCollection<Servico> Servicos { get; set; } = new();
        public ObservableCollection<Agendamento> Agendamentos { get; set; } = new();
        public ObservableCollection<Cliente> ClientesNovos { get; set; } = new();
        public Brush CorFundo
        {
            get
            {
                if (Agendamentos == null || Agendamentos.Count == 0)
                    return Brushes.Transparent;

                // Exemplo: verde se todos pagos, laranja se pendente
                if (Agendamentos.All(a => a.EstaPago))
                    return Brushes.LightGreen;

                return Brushes.Orange;
            }
        }
    }
}
