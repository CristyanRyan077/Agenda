using AgendaNovo.Controles;
using AgendaNovo.Models;
using AgendaNovo.Services;
using AgendaNovo.ViewModels;
using ControlzEx.Standard;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using System.Windows.Threading;
using static Azure.Core.HttpHeader;

namespace AgendaNovo
{
    /// <summary>
    /// Lógica interna para Agendar.xaml
    /// </summary>
    public partial class Agendar : Window
    {
        private ICollectionView _viewClientes;
        private readonly AgendaViewModel _vm;
        public Agendar(AgendaViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;
            Debug.WriteLine($"ViewModel ID: {_vm.GetHashCode()}");
        }
     


        private bool _atualizandoCliente = false;
        private bool _preenchendoViaId = false;

        private void ComboBoxPacotes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var vm = DataContext as AgendaViewModel;
            if (vm?.NovoAgendamento?.PacoteId != null)
            {
                vm.PreencherValorPacoteSelecionado(vm.NovoAgendamento.PacoteId);
            }
        }
            



        private void txtCliente_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // move o foco para fora, disparando LostFocus
                var ui = (UIElement)sender;
                ui.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                e.Handled = true;
            }
        }
        private async void txtIdBusca_LostFocus(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as AgendaViewModel;
            if (vm == null) return;

            if (int.TryParse(txtIdBusca.Text.Trim(), out int id))
            {
                var cliente = vm.ListaClientes.FirstOrDefault(c => c.Id == id);
                if (cliente != null)
                {
                    vm.IgnorarProximoTextChanged = true;
                    vm.ClienteSelecionado = cliente;
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


        private void DatePicker_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            var datePicker = sender as DatePicker;
            if (!datePicker.IsDropDownOpen)
            {
                datePicker.IsDropDownOpen = true;
                e.Handled = true;
            }
        }
    }
}