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

namespace AgendaNovo
{
    /// <summary>
    /// Lógica interna para GerenciarClientes.xaml
    /// </summary>
    public partial class GerenciarClientes : Window
    {
        public GerenciarClientes(AgendaViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        private void txtNascimento_GotFocus(object sender, RoutedEventArgs e)
        {
            calendario.Visibility = Visibility.Visible;
        }

        private void txtNascimento_LostFocus(object sender, RoutedEventArgs e)
        {
            calendario.Visibility = Visibility.Collapsed;
        }

        private void calendario_GotFocus(object sender, RoutedEventArgs e)
        {
            calendario.Visibility = Visibility.Visible;
        }

        private void calendario_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (calendario.SelectedDate.HasValue)
            {
                txtNascimento.Text = calendario.SelectedDate.Value.ToString("dd/MM/yyyy");

            }
        }
    }
}