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

        public MainWindow()
        {
            InitializeComponent();
            vm = new AgendaViewModel();
            this.DataContext = vm;

        }
        private void btnAgenda_Click(object sender, RoutedEventArgs e)
        {
            var agendarWindow = new Agendar(vm);
            agendarWindow.ShowDialog();
        
        } 

        private void btnClientes_Click(object sender, RoutedEventArgs e)
        {
            GerenciarClientes clientes = new GerenciarClientes();
            clientes.ShowDialog();
        }

        private void Calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}