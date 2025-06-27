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
        private AgendaViewModel vm = new AgendaViewModel();

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = vm;

        }
        private void btnAgenda_Click(object sender, RoutedEventArgs e)
        {
            var agendarWindow = new Agendar(vm);
            agendarWindow.ShowDialog();
            /*if (!string.IsNullOrEmpty(Agendar.Agendamento1))
            {
                string agendamento = Agendar.Agendamento1;
                string[] partes = agendamento.Split('|');
                string diaSemanaTexto = partes.Last().Trim();

                switch (diaSemanaTexto)
                {
                    case "Sunday":
                        txt1.Text += agendamento + "\n";
                        break;
                    case "Monday":
                        txt2.Text += agendamento + "\n";
                        break;
                    case "Tuesday":
                        txt3.Text += agendamento + "\n";
                        break;
                    case "Wednesday":
                        txt4.Text += agendamento + "\n";
                        break;
                    case "Thursday":
                        txt5.Text += agendamento + "\n";
                        break;
                    case "Friday":
                        txt6.Text += agendamento + "\n";
                        break;
                    case "Saturday":
                        txt7.Text += agendamento + "\n";
                        break; 

                } 
            } */

        } 

        private void btnClientes_Click(object sender, RoutedEventArgs e)
        {
            GerenciarClientes clientes = new GerenciarClientes();
            clientes.ShowDialog();
        }
    }
}