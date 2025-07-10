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
            vm.Inicializar();
        }

        private void txtCliente_LostFocus(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as AgendaViewModel;
            if (vm == null)
                return;
            var nomeDigitado = (sender as ComboBox)?.Text?.Trim();

            if (string.IsNullOrEmpty(nomeDigitado)) return;

            vm.ClienteSelecionado = vm.ListaClientes.FirstOrDefault(c =>
                c.Nome?.Equals(nomeDigitado, StringComparison.OrdinalIgnoreCase) ?? false);

            txtTel.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
            txtEmail.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
        }
    }
}