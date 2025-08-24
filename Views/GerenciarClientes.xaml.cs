using AgendaNovo.ViewModels;
using AgendaNovo.Views;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AgendaNovo
{
    /// <summary>
    /// Lógica interna para GerenciarClientes.xaml
    /// </summary>
    public partial class GerenciarClientes : Window
    {
        private WindowManager _main;
        public GerenciarClientes(ClienteCriancaViewModel vm, WindowManager main)
        {
            InitializeComponent();
            DataContext = vm;
            _main = main;
            txtCliente.Loaded += (s, e) =>
            {
                if (txtCliente.Template.FindName("PART_EditableTextBox", txtCliente) is TextBox innerTextBox)
                {
                    innerTextBox.PreviewTextInput += txtCliente_PreviewTextInput;
                    DataObject.AddPastingHandler(innerTextBox, TxtCliente_OnPaste);
                }
            };
            Debug.WriteLine($"clientes ViewModel ID: {vm.GetHashCode()}");
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

            _main.GetMainWindow();
        }
        private void btnAgenda_Click(object sender, RoutedEventArgs e)
        {
            FocusManager.SetFocusedElement(this, this);
            Keyboard.ClearFocus();
                _main.GetAgendar();
        }

        private void btnCalendario_Click(object sender, RoutedEventArgs e)
        {
            FocusManager.SetFocusedElement(this, this);
            Keyboard.ClearFocus();

            _main.GetCalendario();
        }



        private void VerificarNomeComCrianca(string textoCompleto)
        {
            var partes = textoCompleto.Split('-', 2, StringSplitOptions.TrimEntries);
            if (partes.Length == 2)
            {
                var vm = DataContext as AgendaViewModel;
                if (vm == null) return;

                if (vm.NovoCliente?.Id == 0 && vm.CriancaSelecionada?.Id == 0)
                {
                    vm.NovoCliente.Nome = partes[0];
                    vm.CriancaSelecionada.Nome = partes[1];

                    txtCliente.Text = partes[0];
                    txtCrianca.Text = partes[1];

                    txtCrianca.Focus();
                    {
                        vm.NovoCliente.Nome = partes[0];
                        vm.CriancaSelecionada.Nome = partes[1];

                        txtCliente.Text = partes[0];
                        txtCrianca.Text = partes[1];

                        txtCrianca.Focus();
                    }
                }
            }
        }

        private void txtCliente_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == "-")
            {
                if (sender is TextBox textBox)
                {
                    string textoAtual = textBox.Text;
                    // Inclui o caractere que está sendo digitado, já que PreviewTextInput ocorre antes de entrar no TextBox
                    string textoCompleto = textoAtual.Insert(textBox.SelectionStart, "-");

                    VerificarNomeComCrianca(textoCompleto);
                    e.Handled = true;
                }
            }

        }

        private void TxtCliente_OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(DataFormats.Text))
            {
                var texto = e.DataObject.GetData(DataFormats.Text) as string;
                if (!string.IsNullOrWhiteSpace(texto) && texto.Contains("-"))
                {
                    VerificarNomeComCrianca(texto);
                    e.CancelCommand();
                }
            }
        }

        private void txtTel_LostFocus(object sender, RoutedEventArgs e)
        {
            if (DataContext is ClienteCriancaViewModel vm)
                vm.DetectarClientePorCampos();
        }

        private void txtEmail_LostFocus(object sender, RoutedEventArgs e)
        {
            if (DataContext is ClienteCriancaViewModel vm)
                vm.DetectarClientePorCampos();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = (ClienteCriancaViewModel)this.DataContext;
            vm.VerificarClientesInativos();
        }

    }
}