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
using Microsoft.Win32;

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





        private void txtCliente_LostFocus(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as AgendaViewModel;
            if (vm == null)
                return;
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
            var vm = DataContext as AgendaViewModel;
            if (vm == null)
                return;
            var pacoteDigitado = (sender as ComboBox)?.Text?.Trim();

            if (string.IsNullOrWhiteSpace(pacoteDigitado))
                return;

            vm.PreencherPacote(pacoteDigitado, valor => { vm.NovoAgendamento.Valor = valor; });
            txtValor.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
        }

        private void btnTema_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Selecionar imagem do tema",
                Filter = "Imagens (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|Todos os arquivos (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                string caminho = dialog.FileName;

                // Supondo que você tenha acesso à ViewModel pelo DataContext
                if (DataContext is AgendaViewModel vm)
                {
                    vm.NovoAgendamento.Tema = caminho;
                }
            }
        }
    }
}
