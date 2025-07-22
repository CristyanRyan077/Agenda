using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AgendaNovo.ViewModels;
using MaterialDesignThemes.Wpf;

namespace AgendaNovo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GerenciarClientes _janelaClientes;
        private Agendar _janelaAgenda;
        public AgendaViewModel vm { get; }

        public GerenciarClientes Clientevm { get; }

        public MainWindow(AgendaViewModel vm)
        {
            InitializeComponent();
            this.vm = vm;
            DataContext = vm;

    
        }
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var vm = DataContext as AgendaViewModel;
            if (vm is null) return;
            if (sender is Border border && border.DataContext is Agendamento agendamento)
            {
                if (MessageBox.Show("Agendamento confirmado e pago?", "Confirmação", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    vm.AtualizarPago(agendamento);
                }

            }
        }

        private void btnAgenda_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is AgendaViewModel vm)
            {
                vm.LimparCamposCommand.Execute(null);
                if (_janelaAgenda == null || !_janelaAgenda.IsLoaded)
                {
                    _janelaAgenda = new Agendar(vm);
                    _janelaAgenda.Show();
                }
                else
                {
                    _janelaAgenda.Focus();
                }
            }
        }

        private void btnClientes_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is AgendaViewModel agendaVm)
            {
                vm.LimparCamposCommand.Execute(null);
                if (_janelaClientes == null || !_janelaClientes.IsLoaded)
                {
                    var clienteVm = new ClienteCriancaViewModel(agendaVm);
                    _janelaClientes = new GerenciarClientes(clienteVm);
                    _janelaClientes.Show();
                }
                else
                {
                    _janelaClientes.Focus();
                }
            }
        }
    }
}