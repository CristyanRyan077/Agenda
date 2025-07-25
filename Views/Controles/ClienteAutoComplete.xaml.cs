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

namespace AgendaNovo.Views.Controles
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
        private void VerificarDuplicados()
        {
            var vm = DataContext as AgendaViewModel;
            if (vm == null) return;

            var nomeDigitado = vm.NomeDigitado?.Trim();
            if (string.IsNullOrWhiteSpace(nomeDigitado))
                return;

            var clientesIguais = vm.ListaClientes
                .Where(c => string.Equals(c.Nome?.Trim(), nomeDigitado, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (clientesIguais.Count > 1)
            {
                var textoDuplicados = string.Join("\n\n", clientesIguais.Select(c =>
                    $"ID: {c.Id}\nNome: {c.Nome}\n" +
                    $"Crianças:\n{string.Join("\n", c.Criancas.Select(cr => $"- {cr.Nome}"))}\n" +
                    $"Telefone: {c.Telefone}\nEmail: {c.Email}"));

                MessageBox.Show($"⚠ Existem vários clientes com esse nome:\n\n{textoDuplicados}",
                                "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);

                vm.ClienteSelecionado = null;
                vm.NomeDigitado = string.Empty;
            }
            else if (clientesIguais.Count == 1)
            {
                vm.ClienteSelecionado = clientesIguais.First();
            }
        }
        private void AutoCompleteBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            AtualizarPlaceholder();
        }
        private void AutoCompleteBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!AutoCompleteBox.IsKeyboardFocusWithin && string.IsNullOrWhiteSpace(AutoCompleteBox.Text))
                PlaceholderText.Visibility = Visibility.Visible;
            var vm = DataContext as AgendaViewModel;
            if (vm != null)
                vm.MostrarSugestoes = false;

            VerificarDuplicados();

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
