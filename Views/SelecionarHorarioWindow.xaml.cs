using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AgendaNovo.Views
{
    /// <summary>
    /// Lógica interna para SelecionarHorarioWindow.xaml
    /// </summary>
    public partial class SelecionarHorarioWindow : Window
    {
        public TimeSpan HorarioSelecionado { get; private set; }

        public SelecionarHorarioWindow(IEnumerable<TimeSpan> horariosLivres, TimeSpan? horarioAtual)
        {
            InitializeComponent();
            cbHorarios.ItemsSource = horariosLivres.Select(h => new Item(h)).ToList();

            // seleciona a primeira sugestão >= atual se existir
            var item = ((IEnumerable<Item>)cbHorarios.ItemsSource)
                        .OrderBy(i => i.Valor)
                        .FirstOrDefault(i => i.Valor >= horarioAtual)
                        ?? ((IEnumerable<Item>)cbHorarios.ItemsSource).FirstOrDefault();

            cbHorarios.SelectedItem = item;
        }
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (cbHorarios.SelectedItem is Item it)
            {
                HorarioSelecionado = it.Valor;
                DialogResult = true;
            }
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private record Item(TimeSpan Valor)
        {
            public override string ToString() => DateTime.Today.Add(Valor).ToString("HH:mm");
        }
    }
}
