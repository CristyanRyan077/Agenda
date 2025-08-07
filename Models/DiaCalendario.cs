using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AgendaNovo.Models
{
    public class DiaCalendario : ObservableObject
    {
        private const int PreviewLimit = 3;
        public IEnumerable<Agendamento> PreviewAgendamentos
        => Agendamentos.Take(PreviewLimit);
        public int OverflowCount
        => Math.Max(0, Agendamentos.Count - PreviewLimit);
        public bool HasOverflow
    => OverflowCount > 0;

        public IEnumerable<object> PreviewItemsWithOverflow
        {
            get
            {
                foreach (var ag in Agendamentos.Take(PreviewLimit))
                    yield return ag;
                if (OverflowCount > 0)
                    yield return OverflowText;
            }
        }
        public string OverflowText
    => OverflowCount > 0 ? $"+{OverflowCount} mais" : string.Empty;
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
        public DiaCalendario(DateTime data)
        {
            Data = data;
            Agendamentos.CollectionChanged += (_, __) =>
            {
                OnPropertyChanged(nameof(PreviewAgendamentos));
                OnPropertyChanged(nameof(OverflowCount));
                OnPropertyChanged(nameof(OverflowText));
                OnPropertyChanged(nameof(PreviewItemsWithOverflow));
                OnPropertyChanged(nameof(CorFundo));
                OnPropertyChanged(nameof(HasOverflow));
            };

        }

    }
}
