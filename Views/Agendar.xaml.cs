using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using AgendaNovo.Models;
using ControlzEx.Standard;
using static Azure.Core.HttpHeader;

namespace AgendaNovo
{
    /// <summary>
    /// Lógica interna para Agendar.xaml
    /// </summary>
    public partial class Agendar : Window
    {
        private ICollectionView _viewClientes;
        public Agendar(AgendaViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            vm.PropertyChanged += Vm_PropertyChanged;
            var cv = CollectionViewSource.GetDefaultView(vm.ListaAgendamentos);
            cv.Filter = null;
            cv.Refresh();
            vm.Inicializar();

            _viewClientes = CollectionViewSource.GetDefaultView(((AgendaViewModel)DataContext).ListaClientes);
            _viewClientes.Filter = ClienteFilter;
            txtCliente.ItemsSource = _viewClientes;
            txtCliente.Loaded += (s, e) =>
            {
                var editor = FindVisualChild<TextBox>(txtCliente);
                if (editor == null) return;
                txtCliente.AddHandler(
                      UIElement.GotKeyboardFocusEvent,
                      new KeyboardFocusChangedEventHandler((s2, e2) =>
                      {
                          // só quando a origem for o TextBox interno
                          if (e2.OriginalSource is TextBox tb && tb == editor)
                          {
                              // agenda depois do SelectAll interno
                              tb.Dispatcher.BeginInvoke(new Action(() =>
                              {
                                  tb.SelectionLength = 0;
                                  tb.CaretIndex = tb.Text.Length;
                              }), System.Windows.Threading.DispatcherPriority.ContextIdle);
                          }
                      }),
                      handledEventsToo: true);

                editor.GotFocus += (s2, e2) =>
                {
                    // tira qualquer seleção e leva o caret pro fim
                    editor.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        editor.SelectionLength = 0;
                        editor.CaretIndex = editor.Text.Length;
                    }),
                    System.Windows.Threading.DispatcherPriority.ContextIdle);
                };

                // a cada mudança de texto, limpa a seleção antiga e reabre o dropdown
                editor.TextChanged += (s2, e2) =>
                {
                    if (_preenchendoViaId) return;
                    _viewClientes.Refresh();
                    txtCliente.IsDropDownOpen = !string.IsNullOrEmpty(editor.Text);
                };

                editor.PreviewKeyDown += (s2, e2) =>
                {
                    if (editor.SelectionLength > 0 &&
                        (e2.Key >= Key.A && e2.Key <= Key.Z ||
                         e2.Key >= Key.D0 && e2.Key <= Key.D9))
                    {
                        editor.SelectionLength = 0;
                        editor.CaretIndex = editor.Text.Length;
                    }
                };
            };
        }
        private void Vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AgendaViewModel.ClienteSelecionado))
            {
                var vm = (AgendaViewModel)sender;
                // Force a atualização do Combo:
                txtCliente.Dispatcher.BeginInvoke(new Action(() =>
                {
                    txtCliente.SelectedItem = vm.ClienteSelecionado;
                    // 2) “reinicia” o Text para o nome EXATO do cliente
                    txtCliente.Text = vm.ClienteSelecionado?.Nome ?? "";
                }), DispatcherPriority.ContextIdle);
            }
        }
        private void Tb_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var tb = (TextBox)sender;
            if (!tb.IsKeyboardFocusWithin)
            {
                e.Handled = true;
                tb.Focus();
            }
        }
        private void txtCliente_KeyUp(object sender, KeyEventArgs e)
        {
            _viewClientes.Refresh();
            if (!string.IsNullOrEmpty(txtCliente.Text))
                txtCliente.IsDropDownOpen = true;
        }
        private bool ClienteFilter(object obj)
        {
            if (obj is Cliente c)
            {
                var texto = txtCliente.Text?.Trim() ?? "";
                if (string.IsNullOrEmpty(texto))
                    return true;   // lista tudo quando vazio

                // só nomes que COMEÇAM com o que digitou
                return c.Nome.StartsWith(texto, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t) return t;
                var result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        private void EditableTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }



        private bool _atualizandoCliente = false;
        private bool _preenchendoViaId = false;
       


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
        private void dgAgendamentos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Assim que trocar o registro, limpamos o Text do Combo
            txtCliente.Text = string.Empty;
            txtCliente.IsDropDownOpen = false;
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
    }
}