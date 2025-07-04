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
            var vm = (AgendaViewModel)DataContext;
            var nomeDigitado = (sender as ComboBox)?.Text?.Trim();

            if (string.IsNullOrEmpty(nomeDigitado)) return;

            // Encontra o cliente (com comparação case insensitive)
            vm.ClienteSelecionado = vm.ListaClientes.FirstOrDefault(c =>
                c.Nome?.Equals(nomeDigitado, StringComparison.OrdinalIgnoreCase) ?? false);

            // Atualiza os bindings
            txtTelefone.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
            txtcrianca.GetBindingExpression(ComboBox.TextProperty)?.UpdateTarget();
            txtcrianca.GetBindingExpression(ComboBox.SelectedItemProperty)?.UpdateTarget();
        }


        private void txtpacote_LostFocus(object sender, RoutedEventArgs e)
        {
            var vm = (AgendaViewModel)this.DataContext;
            var pacoteDigitado = (sender as ComboBox)?.Text?.Trim();

            vm.PreencherPacote(pacoteDigitado, valor => { vm.NovoAgendamento.Valor = valor; });
            txtValor.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
        }

        private void txtcrianca_LostFocus(object sender, RoutedEventArgs e)
        {
            var vm = (AgendaViewModel)DataContext;
            var nomeDigitado = (sender as ComboBox)?.Text?.Trim();

            if (string.IsNullOrEmpty(nomeDigitado))
                return;

            var criancaExistente = vm.ListaCriancas
                .FirstOrDefault(c => string.Equals(c.Nome, nomeDigitado, StringComparison.OrdinalIgnoreCase));

            if (criancaExistente != null)
            {
                vm.NovoAgendamento.Crianca = criancaExistente;
            }
            else
            {
<<<<<<< HEAD
                vm.NovoAgendamento.Crianca = new Crianca
                {
                    Nome = criancaExistente.Nome,
                    Idade = criancaExistente.Idade,
                    Genero = criancaExistente.Genero
                };
=======
                vm.NovoAgendamento.Crianca = new Crianca { Nome = nomeDigitado };
>>>>>>> 339a18d4c7781204c8c80dd673286d34e4ee8714
                // Opcional: adicionar na lista para aparecer no autocomplete
                vm.ListaCriancas.Add(vm.NovoAgendamento.Crianca);
            }

            // Atualiza binding para garantir a UI sincronizada
            txtcrianca.GetBindingExpression(ComboBox.TextProperty)?.UpdateTarget();
        }
    }
}
