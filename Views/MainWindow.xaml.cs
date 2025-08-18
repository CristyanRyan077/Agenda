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

        private GerenciarClientes _janelaClientes;
        private Agendar _janelaAgenda;
        private Calendario _calendario;
        public AgendaViewModel vm { get; }

        public GerenciarClientes Clientevm { get; }

        public MainWindow(AgendaViewModel agendaVm, IServiceProvider sp)
        {
            InitializeComponent();
            vm = agendaVm;
            DataContext = vm;
            Debug.WriteLine($"MainWindow ViewModel ID: {vm.GetHashCode()}");
            _sp = sp;
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
                if (_janelaAgenda == null || !_janelaAgenda.IsLoaded)
                {
                    _janelaAgenda = new Agendar(vm, this._sp); 
                    _janelaAgenda.Closed += (s, ev) => _janelaAgenda = null;
                    _janelaAgenda.Show();
                }
                else
                {
                    if (_janelaAgenda.WindowState == WindowState.Minimized)
                        _janelaAgenda.WindowState = WindowState.Normal;
                    _janelaAgenda.Activate();
                }
            }
        }

        private void btnClientes_Click(object sender, RoutedEventArgs e)
        {
            FocusManager.SetFocusedElement(this, this);
            Keyboard.ClearFocus();

            if (_janelaClientes == null || !_janelaClientes.IsLoaded)
            {
                _janelaClientes = _sp.GetRequiredService<GerenciarClientes>();
                _janelaClientes.Closed += (s, ev) => _janelaClientes = null;
                _janelaClientes.Show();
            }
            else
            {
                if (_janelaClientes.WindowState == WindowState.Minimized)
                    _janelaClientes.WindowState = WindowState.Normal;
                _janelaClientes.Activate();
            }
        }

        private void btnCalendario_Click(object sender, RoutedEventArgs e)
        {
            FocusManager.SetFocusedElement(this, this);
            Keyboard.ClearFocus();

            if (_calendario == null || !_calendario.IsLoaded)
            {
                _calendario = _sp.GetRequiredService<Calendario>();
                _calendario.Closed += (s, ev) => _calendario = null;
                _calendario.Show();
            }
            else
            {
                if (_calendario.WindowState == WindowState.Minimized)
                    _calendario.WindowState = WindowState.Normal;
                _calendario.Activate();
            }
        }
    }
}