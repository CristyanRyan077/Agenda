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
            if (vm.IgnorarProximoTextChanged)
            {
                vm.IgnorarProximoTextChanged = false;
                return;
            }
            vm.UsuarioDigitouNome = true;
            AtualizarPlaceholder();
        }
        private void AutoCompleteBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!AutoCompleteBox.IsKeyboardFocusWithin && string.IsNullOrWhiteSpace(AutoCompleteBox.Text))
                PlaceholderText.Visibility = Visibility.Visible;
            var vm = DataContext as AgendaViewModel;
            if (vm != null)
                vm.MostrarSugestoes = false;

        }

        private void AutoCompleteBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PlaceholderText.Visibility = Visibility.Collapsed;
        }
        private void AtualizarPlaceholder()
        {
            PlaceholderText.Visibility = string.IsNullOrWhiteSpace(AutoCompleteBox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

    }


}
