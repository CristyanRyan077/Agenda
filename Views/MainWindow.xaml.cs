using System.Diagnostics;
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
using AgendaNovo.Views;
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

        private WindowManager _main;
        public AgendaViewModel vm { get; }

        public GerenciarClientes Clientevm { get; }

        public MainWindow(AgendaViewModel agendaVm, WindowManager main)
        {
            InitializeComponent();
            vm = agendaVm;
            DataContext = vm;
            Debug.WriteLine($"MainWindow ViewModel ID: {vm.GetHashCode()}");
            _main = main;
            this.Closed += (s, e) => Application.Current.Shutdown();


        }
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var vm = DataContext as AgendaViewModel;
            if (vm is null) return;
            
        }

        private void btnAgenda_Click(object sender, RoutedEventArgs e)
        {
            FocusManager.SetFocusedElement(this, this);
            Keyboard.ClearFocus();

            if (DataContext is AgendaViewModel vm)
            {
                _main.GetAgendar();
            }
        }

        private void btnClientes_Click(object sender, RoutedEventArgs e)
        {
            FocusManager.SetFocusedElement(this, this);
            Keyboard.ClearFocus();

           _main.GetGerenciarClientes();
        }

        private void btnCalendario_Click(object sender, RoutedEventArgs e)
        {
            FocusManager.SetFocusedElement(this, this);
            Keyboard.ClearFocus();

           _main.GetCalendario();
        }
    }
}