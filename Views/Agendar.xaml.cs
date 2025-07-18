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
using static Azure.Core.HttpHeader;

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
                DataContext = vm;
            }




        private bool _atualizandoCliente = false;
        private void txtCliente_LostFocus(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as AgendaViewModel;
            if (vm == null)
                return;
            if (_atualizandoCliente) return;

            _atualizandoCliente = true;

            var comboBox = sender as ComboBox;
            var nomeDigitado = comboBox?.Text?.Trim();

            if (string.IsNullOrEmpty(nomeDigitado)) return;
            var clientesIguais = vm.ListaClientes
            .Where(c => string.Equals(c.Nome?.Trim(), nomeDigitado, StringComparison.OrdinalIgnoreCase))
            .ToList();

            if (clientesIguais.Count > 1)
            {
                var texto = string.Join("\n\n", clientesIguais.Select(c =>
                    $"ID: {c.Id}\nNome: {c.Nome}\n" +
                    $"Crianças:\n{string.Join("\n", c.Criancas.Select(cr => $"- {cr.Nome}"))}\n" +
                    $"Telefone: {c.Telefone}\nEmail: {c.Email}"));

                MessageBox.Show($"Existem vários clientes com esse nome:\n\n{texto}", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);

                // Apaga o campo para evitar repetição
                comboBox.Text = string.Empty;

                // Opcional: também pode limpar o cliente selecionado
                vm.ClienteSelecionado = null;

                return;
            }

            // Se só um cliente foi encontrado, seleciona ele normalmente
            vm.ClienteSelecionado = clientesIguais.FirstOrDefault();

    

            // Atualiza os bindings
            txtTelefone.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
            txtcrianca.GetBindingExpression(ComboBox.TextProperty)?.UpdateTarget();
            txtcrianca.GetBindingExpression(ComboBox.SelectedItemProperty)?.UpdateTarget();
            _atualizandoCliente = false;
        }


        private void txtpacote_LostFocus(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as AgendaViewModel;
            if (vm == null)
                return;
            var pacoteDigitado = (sender as ComboBox)?.Text?.Trim();

            if (string.IsNullOrWhiteSpace(pacoteDigitado))
                return;

            vm.PreencherPacote(pacoteDigitado, valor => { vm.NovoAgendamento.Valor = valor; });
            txtValor.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
        }

        private void txtIdBusca_LostFocus(object sender, RoutedEventArgs e)
        {

            var vm = DataContext as AgendaViewModel;
            if (vm == null) return;

            if (int.TryParse(txtIdBusca.Text.Trim(), out int id))
            {
                var cliente = vm.ListaClientes.FirstOrDefault(c => c.Id == id);
                if (cliente != null)
                {
                    vm.ClienteSelecionado = cliente;
                }
                else
                {
                    MessageBox.Show("Cliente com esse ID não encontrado.");
                }
            }
        }
    }
}
