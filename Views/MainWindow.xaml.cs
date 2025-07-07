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
using MaterialDesignThemes.Wpf;

namespace AgendaNovo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public AgendaViewModel vm { get; }

        public MainWindow(AgendaViewModel vm)
        {
            InitializeComponent();

            var context = new AgendaContext();
            vm = new AgendaViewModel(context);
            DataContext = vm;

            Loaded += async (_, __) =>
            {
                await Task.Run(() => vm.CarregarDadosDoBanco());
            };
        }

        private void btnAgenda_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is AgendaViewModel vm)
            {
                var agendarWindow = new Agendar(vm);
                agendarWindow.ShowDialog();
            }
        }

        private void btnClientes_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is AgendaViewModel vm)
            {
                GerenciarClientes clientes = new GerenciarClientes(vm);
                clientes.ShowDialog();
            }
        }
    }
}