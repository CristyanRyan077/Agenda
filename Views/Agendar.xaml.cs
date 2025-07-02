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
using AgendaNovo.Models;

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





        private void txtCliente_LostFocus(object sender, RoutedEventArgs e)
        {
            var vm = (AgendaViewModel)this.DataContext;
            var nomeDigitado = (sender as ComboBox)?.Text?.Trim();

            vm.PreencherCamposSeClienteExistir(nomeDigitado, cliente => { vm.NovoCliente.Telefone = cliente.Telefone; });

            txtTelefone.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
        }

        private void txtpacote_LostFocus(object sender, RoutedEventArgs e)
        {
            var vm = (AgendaViewModel)this.DataContext;
            var pacoteDigitado = (sender as ComboBox)?.Text?.Trim();

            vm.PreencherPacote(pacoteDigitado, valor => { vm.NovoAgendamento.Valor = valor; });
            txtValor.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
        }
    }
}
