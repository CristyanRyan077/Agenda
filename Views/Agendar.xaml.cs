using AgendaNovo.Controles;
using AgendaNovo.Models;
using AgendaNovo.Services;
using AgendaNovo.ViewModels;
using AgendaNovo.Views;
using ControlzEx.Standard;
using Microsoft.Extensions.DependencyInjection;
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
using System.Windows.Media.Animation;
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
        public Agendar(AgendaViewModel vm, IServiceProvider sp)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;
            Debug.WriteLine($"ViewModel ID: {_vm.GetHashCode()}");
            _sp = sp;
        }

        private readonly IServiceProvider _sp;
        private GerenciarClientes _janelaClientes;
        private MainWindow _mainwindow;
        private Calendario _calendario;
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
        private void btnToggle_Checked(object sender, RoutedEventArgs e)
        {
            var anim = new DoubleAnimation(0, TimeSpan.FromMilliseconds(250));
            DrawerTransform.BeginAnimation(TranslateTransform.XProperty, anim);
        }

        private void btnToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            var anim = new DoubleAnimation(-200, TimeSpan.FromMilliseconds(250));
            DrawerTransform.BeginAnimation(TranslateTransform.XProperty, anim);
        }
        private void btnMainwindow_Click(object sender, RoutedEventArgs e)
        {
            FocusManager.SetFocusedElement(this, this);
            Keyboard.ClearFocus();

            if (_mainwindow == null || !_mainwindow.IsLoaded)
            {
                _mainwindow = _sp.GetRequiredService<MainWindow>();
                _mainwindow.Closed += (s, ev) => _mainwindow = null;
                _mainwindow.Show();
            }
            else
            {
                if (_mainwindow.WindowState == WindowState.Minimized)
                    _mainwindow.WindowState = WindowState.Normal;
                _mainwindow.Activate();
            }
        }
        private void btnClientes_Click(object sender, RoutedEventArgs e)
        {
            FocusManager.SetFocusedElement(this, this);
            Keyboard.ClearFocus();

            if (_janelaClientes == null || !_janelaClientes.IsLoaded)
            {
                _janelaClientes = _sp.GetRequiredService<GerenciarClientes>();
                _janelaClientes.Closed += (s, ev) => _janelaClientes = null;
                _janelaClientes.Show();
            }
            else
            {
                if (_janelaClientes.WindowState == WindowState.Minimized)
                    _janelaClientes.WindowState = WindowState.Normal;
                _janelaClientes.Activate();
            }
        }

        private void btnCalendario_Click(object sender, RoutedEventArgs e)
        {
            FocusManager.SetFocusedElement(this, this);
            Keyboard.ClearFocus();

            if (_calendario == null || !_calendario.IsLoaded)
            {
                _calendario = _sp.GetRequiredService<Calendario>();
                _calendario.Closed += (s, ev) => _calendario = null;
                _calendario.Show();
            }
            else
            {
                if (_calendario.WindowState == WindowState.Minimized)
                    _calendario.WindowState = WindowState.Normal;
                _calendario.Activate();
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