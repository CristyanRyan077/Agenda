using AgendaNovo.Interfaces;
using AgendaNovo.ViewModels;
using AgendaNovo.Views;
using CommunityToolkit.Mvvm.Messaging;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;
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
using static AgendaNovo.AgendaViewModel;

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
            WeakReferenceMessenger.Default.Register<FocusAgendamentoMessage>(this, async (_, msg) =>
            {
                if (DataContext is not AgendaViewModel vm) return;

                // 1) Leva a janela para a semana do alvo (isso preenche os ItemsControls certos)
                vm.DataReferencia = msg.Data;

                // 2) Aguarda layout atualizar (senão o container ainda não existe)
                await System.Windows.Threading.Dispatcher
                .Yield(System.Windows.Threading.DispatcherPriority.Background);

                BringCardIntoView(msg.AgendamentoId);
            });
            this.Closed += (s, e) => Application.Current.Shutdown();


        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is DependencyObject d)
            {
                for (var p = d; p != null; p = VisualTreeHelper.GetParent(p))
                {
                    if (p is Button) return; // NÃO marcar e.Handled
                }
            }
            var vm = DataContext as AgendaViewModel;
            if (vm is null) return;
            Debug.WriteLine("[BTN] Cliquei!");

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


        private void btnFinanceiro_Click(object sender, RoutedEventArgs e)
        {
            FocusManager.SetFocusedElement(this, this);
            Keyboard.ClearFocus();

            _main.AbrirFinanceiroNaMainAsync();
        }
        private void btnFotos_Click(object sender, RoutedEventArgs e)
        {
            FocusManager.SetFocusedElement(this, this);
            Keyboard.ClearFocus();

            _main.GetFotos();
            
        }
       
        private void BringCardIntoView(int agendamentoId)
        {
            // Decide o ItemsControl pelo DayOfWeek do item
            // Primeiro, tenta achar o item na memória:
            if (DataContext is not AgendaViewModel vm) return;

            Agendamento? alvo =
                vm.AgendamentosDomingo.Concat(vm.AgendamentosSegunda)
                  .Concat(vm.AgendamentosTerca).Concat(vm.AgendamentosQuarta)
                  .Concat(vm.AgendamentosQuinta).Concat(vm.AgendamentosSexta)
                  .Concat(vm.AgendamentosSabado)
                  .FirstOrDefault(a => a.Id == agendamentoId);

            if (alvo is null) return;

            ItemsControl items = alvo.Data.DayOfWeek switch
            {
                DayOfWeek.Monday => ItemsSegunda,
                DayOfWeek.Tuesday => ItemsTerca,
                DayOfWeek.Wednesday => ItemsQuarta,
                DayOfWeek.Thursday => ItemsQuinta,
                DayOfWeek.Friday => ItemsSexta,
                DayOfWeek.Saturday => ItemsSabado,
                _ => ItemsDomingo
            };

            // Gera o container e rola até ele
            var container = items.ItemContainerGenerator.ContainerFromItem(alvo) as FrameworkElement;
            if (container == null)
            {
                // Se virtualizado, força gerar:
                items.UpdateLayout();
                container = items.ItemContainerGenerator.ContainerFromItem(alvo) as FrameworkElement;
            }
            container?.BringIntoView();

            // “flash” para o item atual (opcional)
            FlashBorder(container);
        }

        private async void FlashBorder(FrameworkElement? fe)
        {
            if (fe is null) return;
            var b = fe as Border ?? fe.FindName("RootBorder") as Border;
            if (b == null) return;

            var old = b.BorderBrush;
            var oldThick = b.BorderThickness;

            b.BorderBrush = Brushes.Gold;
            b.BorderThickness = new Thickness(3);
            await Task.Delay(500);
            b.BorderBrush = old;
            b.BorderThickness = oldThick;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[BTN] Click roteado ao Button");
        }
    }
}