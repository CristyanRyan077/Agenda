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
using AgendaNovo.Interfaces;
using AgendaNovo.ViewModels;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;

namespace AgendaNovo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IClienteService _clienteService;
        private readonly ICriancaService _criancaService;
        private readonly IAgendamentoService _agendamentoService;

        private readonly IServiceProvider _sp;

        private GerenciarClientes _janelaClientes;
        private Agendar _janelaAgenda;
        public AgendaViewModel vm { get; }

        public GerenciarClientes Clientevm { get; }

        public MainWindow(AgendaViewModel agendaVm, IServiceProvider sp)
        {
            InitializeComponent();
            vm = agendaVm;
            DataContext = vm;
            _sp = sp;
            this.Closed += (s, e) => Application.Current.Shutdown();


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
                    var main = _sp.GetRequiredService<Agendar>();
                    main.Show();
                }
                else
                {
                    _janelaAgenda.Focus();
                }
            }
        }

        private void btnClientes_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as AgendaViewModel)?.LimparCamposCommand.Execute(null);

            if (_janelaClientes == null || !_janelaClientes.IsLoaded)
            {
                // em vez de new, resolve via DI
                _janelaClientes = _sp.GetRequiredService<GerenciarClientes>();
                _janelaClientes.Owner = this;
                _janelaClientes.Show();


            }
            else
            {
                _janelaClientes.Focus();
            }
        }
    }
}