using AgendaNovo.Models;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AgendaNovo.Controles
{
    /// <summary>
    /// Interação lógica para ServicosAutoComplete.xam
    /// </summary>
    public partial class ServicosAutoComplete : UserControl
    {
        public ServicosAutoComplete()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }
        private Window? _parentWindow;
        private void OnWindowDeactivated(object? sender, EventArgs e) => FecharPopup();
        private void OnWindowStateChanged(object? sender, EventArgs e) => FecharPopup();
        private void OnWindowLocationOrSizeChanged(object? sender, EventArgs e) => FecharPopup();
        private void OnAppDeactivated(object? sender, EventArgs e) => FecharPopup();
        private void AutoCompleteBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is AgendaViewModel vm)
            {
                vm.CarregarServicos();
                if (string.IsNullOrWhiteSpace(vm.ServicoDigitado))
                    vm.FiltrarServicos(string.Empty);

                vm.MostrarSugestoesServico = true;
            }
        }
        private void OnLoaded(object s, RoutedEventArgs e)
        {
            _parentWindow = Window.GetWindow(this);
            if (_parentWindow != null)
            {
                _parentWindow.Deactivated += OnWindowDeactivated;
                _parentWindow.StateChanged += OnWindowStateChanged;
                _parentWindow.LocationChanged += OnWindowLocationOrSizeChanged;
                _parentWindow.SizeChanged += OnWindowLocationOrSizeChanged;
            }

            // Fecha quando o APP perde o foco (alt+tab, troca de app)
            Application.Current.Deactivated += OnAppDeactivated;
        }
        private void OnUnloaded(object s, RoutedEventArgs e)
        {
            if (_parentWindow != null)
            {
                _parentWindow.Deactivated -= OnWindowDeactivated;
                _parentWindow.StateChanged -= OnWindowStateChanged;
                _parentWindow.LocationChanged -= OnWindowLocationOrSizeChanged;
                _parentWindow.SizeChanged -= OnWindowLocationOrSizeChanged;
                _parentWindow = null;
            }
            Application.Current.Deactivated -= OnAppDeactivated;
        }
        private void FecharPopup()
        {
            var vm = DataContext as AgendaViewModel;
            if (vm != null)
                vm.MostrarSugestoesServico = false; // Fecha o Popup
        }

        private void AutoCompleteBox_TextChanged(object sender, TextChangedEventArgs e)
        {

            var vm = DataContext as AgendaViewModel;
            if (vm == null)
                return;
            AtualizarPlaceholder();
            if (vm.ResetandoCampos)
                return;
            vm.UsuarioDigitouNome = true;
            
        }
        private void AutoCompleteBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!AutoCompleteBox.IsKeyboardFocusWithin && string.IsNullOrWhiteSpace(AutoCompleteBox.Text))
                PlaceholderText.Visibility = Visibility.Visible;
            FecharPopup();

        }

        private void AutoCompleteBox_GotFocus(object sender, RoutedEventArgs e)
        {

            if (DataContext is AgendaViewModel vm && vm.ResetandoCampos)
                return;

            PlaceholderText.Visibility = Visibility.Collapsed;

            if (DataContext is AgendaViewModel vm2)
            {
                if (string.IsNullOrWhiteSpace(vm2.ServicoDigitado))
                    vm2.FiltrarServicos(string.Empty);

                vm2.MostrarSugestoesServico = true;
            }
        }
        public void AtualizarPlaceholder()
        {
            if (string.IsNullOrWhiteSpace(AutoCompleteBox.Text))
            {
                PlaceholderText.Visibility = AutoCompleteBox.IsKeyboardFocusWithin
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }
            else
            {
                PlaceholderText.Visibility = Visibility.Collapsed;
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is AgendaViewModel vm && sender is ListBox lb && lb.SelectedItem is Servico s)
            {
                vm.IgnorarProximoTextChanged = true;
                vm.ServicoDigitado = s.Nome; // atualiza o texto visível
                vm.ServicoSelecionado = s;
                vm.MostrarSugestoesServico = false;
                AutoCompleteBox.CaretIndex = AutoCompleteBox.Text?.Length ?? 0;
            }
        }

    }


}
