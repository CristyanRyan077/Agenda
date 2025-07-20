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
        private bool _preenchendoViaId = false;
        private void txtCliente_LostFocus(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as AgendaViewModel;

            if (vm == null)
                return;
            if (_preenchendoViaId || string.Equals(txtIdBusca.IsFocused, true))
            {
                _preenchendoViaId = false;
                return;
            }
            if (_atualizandoCliente) return;
            _atualizandoCliente = true;

            var comboBox = sender as ComboBox;
            var nomeDigitado = comboBox?.Text?.Trim();
            var nomeLimpo = nomeDigitado?.Split(" (ID:")[0].Trim();

            if (string.IsNullOrEmpty(nomeDigitado))
            {
                vm.ClienteSelecionado = null;
                _atualizandoCliente = false;
                return;
            }
            var clienteExato = vm.ListaClientes
             .FirstOrDefault(c =>
            string.Equals(c.Nome?.Trim(), nomeLimpo, StringComparison.OrdinalIgnoreCase));

            if (clienteExato == null)
            {
                vm.ClienteSelecionado = null;
                _atualizandoCliente = false;
                return;
            }

            var clientesIguais = vm.ListaClientes
                .Where(c => string.Equals(c.Nome?.Trim(), nomeLimpo, StringComparison.OrdinalIgnoreCase))
                .ToList();


            if (clientesIguais.Count > 1)
            {
                var texto = string.Join("\n\n", clientesIguais.Select(c =>
                    $"ID: {c.Id}\nNome: {c.Nome}\n" +
                    $"Crianças:\n{string.Join("\n", c.Criancas.Select(cr => $"- {cr.Nome}"))}\n" +
                    $"Telefone: {c.Telefone}\nEmail: {c.Email}"));

                MessageBox.Show($"Existem vários clientes com esse nome:\n\n{texto}", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                comboBox.Text = string.Empty;
                vm.ClienteSelecionado = null;
                _atualizandoCliente = false;
                return;
            }
            vm.ClienteSelecionado = clienteExato;
            _preenchendoViaId = true;

            vm.NovoCliente.Id = clienteExato.Id;
            vm.NovoCliente.Nome = clienteExato.Nome;
            vm.NovoCliente.Telefone = clienteExato.Telefone;
            vm.NovoCliente.Email = clienteExato.Email;

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
                    _preenchendoViaId = true;
                    vm.ClienteSelecionado = cliente;
                    txtCliente.GetBindingExpression(ComboBox.TextProperty)?.UpdateTarget();
                    txtTelefone.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
                    txtcrianca.GetBindingExpression(ComboBox.TextProperty)?.UpdateTarget();
                    txtcrianca.GetBindingExpression(ComboBox.SelectedItemProperty)?.UpdateTarget();
                }
                else
                {
                    MessageBox.Show("Cliente com esse ID não encontrado.");
                }
            }
        }
    }
}
