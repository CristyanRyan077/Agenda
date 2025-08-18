using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AgendaNovo.Controles
{
    /// <summary>
    /// Interação lógica para ClienteAutoComplete.xam
    /// </summary>
    public partial class ClienteAutoComplete : UserControl
    {
        public ClienteAutoComplete()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                var parentWindow = Window.GetWindow(this);
                if (parentWindow != null)
                {
                    parentWindow.Deactivated += (sender, args) => FecharPopup();
                    parentWindow.StateChanged += (sender, args) => FecharPopup();
                }
            };
        }
        private void FecharPopup()
        {
            var vm = DataContext as AgendaViewModel;
            if (vm != null)
                vm.MostrarSugestoes = false; // Fecha o Popup
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
            AtualizarPlaceholder();
        }
        private void AutoCompleteBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!AutoCompleteBox.IsKeyboardFocusWithin && string.IsNullOrWhiteSpace(AutoCompleteBox.Text))
                PlaceholderText.Visibility = Visibility.Visible;
            var vm = DataContext as AgendaViewModel;
            if (vm != null )
                vm.MostrarSugestoes = false;

        }

        private void AutoCompleteBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as AgendaViewModel;
            if (vm.ResetandoCampos)
                return;
            PlaceholderText.Visibility = Visibility.Collapsed;
            if (DataContext is AgendaViewModel vm2)
            {

                vm2.MostrarSugestoes = true;
            }
        }
        private void AtualizarPlaceholder()
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

    }
}
