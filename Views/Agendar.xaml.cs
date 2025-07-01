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
    /// Lógica interna para Agendar.xaml
    /// </summary>
        public partial class Agendar : Window
        {

            public Agendar(AgendaViewModel vm)
            {
                InitializeComponent();
                this.DataContext = vm;
            }



        private void ComboBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var vm = (AgendaViewModel)this.DataContext;
            var nome = vm.NovoCliente?.Nome;
            string? nomeDigitado = (sender as ComboBox)?.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(nomeDigitado))
            {
                var existente = vm.ListaClientes.FirstOrDefault(c => string.Equals(c.Nome.Trim(), nomeDigitado, StringComparison.OrdinalIgnoreCase));
                if (existente is not null && existente.Nome.Equals(nomeDigitado, StringComparison.OrdinalIgnoreCase))
                {
                    vm.NovoCliente.Telefone = existente.Telefone;
                    //vm.NovoCliente.Crianca = existente.Crianca;
                }
            }
            BindingExpression be = txtTelefone.GetBindingExpression(ComboBox.TextProperty);
            be?.UpdateTarget();
        }
    }
}
