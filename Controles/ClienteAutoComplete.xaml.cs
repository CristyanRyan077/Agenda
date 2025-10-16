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
            var dc = DataContext;
            if (dc == null) return;

            AtualizarPlaceholder();

            // tenta ler ResetandoCampos
            var propReset = dc.GetType().GetProperty("ResetandoCampos");
            if (propReset != null && propReset.PropertyType == typeof(bool))
            {
                var reset = (bool)propReset.GetValue(dc);
                if (reset) return;
            }

            // tenta setar UsuarioDigitouNome
            var propUsuarioDigitou = dc.GetType().GetProperty("UsuarioDigitouNome");
            if (propUsuarioDigitou != null && propUsuarioDigitou.PropertyType == typeof(bool))
            {
                propUsuarioDigitou.SetValue(dc, true);
            }

            AtualizarPlaceholder();
        }
        private void AutoCompleteBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!AutoCompleteBox.IsKeyboardFocusWithin && string.IsNullOrWhiteSpace(AutoCompleteBox.Text))
                PlaceholderText.Visibility = Visibility.Visible;

            var dc = DataContext;
            if (dc == null) return;

            // tenta setar MostrarSugestoes = false
            var propMostrarSug = dc.GetType().GetProperty("MostrarSugestoes");
            if (propMostrarSug != null && propMostrarSug.PropertyType == typeof(bool))
            {
                propMostrarSug.SetValue(dc, false);
            }
        }

        private void AutoCompleteBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var dc = DataContext;
            if (dc is null) return;

            // tenta ler ResetandoCampos se existir
            var propReset = dc.GetType().GetProperty("ResetandoCampos");
            if (propReset != null && propReset.PropertyType == typeof(bool))
            {
                var reset = (bool)propReset.GetValue(dc);
                if (reset) return;
            }

            if (PlaceholderText != null)
                PlaceholderText.Visibility = Visibility.Collapsed;

            // tenta setar MostrarSugestoes se existir
            var propMostrarSug = dc.GetType().GetProperty("MostrarSugestoes");
            if (propMostrarSug != null && propMostrarSug.PropertyType == typeof(bool))
            {
                propMostrarSug.SetValue(dc, true);
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
