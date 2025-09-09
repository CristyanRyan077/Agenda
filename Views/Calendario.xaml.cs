using AgendaNovo.Controles;
using AgendaNovo.Models;
using AgendaNovo.ViewModels;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AgendaNovo.Views
{
    /// <summary>
    /// Lógica interna para Calendario.xaml
    /// </summary>
    public partial class Calendario : Window
    {
        private WindowManager _main;
        public Calendario(CalendarioViewModel vm, WindowManager main)
        {
            InitializeComponent();
            DataContext = vm;
            _main = main;
        }
        private static T FindAncestorByName<T>(DependencyObject current, string name) where T : FrameworkElement
        {
            while (current != null)
            {
                if (current is T fe && fe.Name == name) return fe;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        private void Dia_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is DependencyObject d && FindAncestorByName<Border>(d, "ItemBorder") != null)
                return;

            if (sender is Border && ((FrameworkElement)sender).DataContext is DiaCalendario dia
                && DataContext is CalendarioViewModel vm)
            {
                vm.SelecionarDia(dia.Data);
            }
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
        private void btnClientes_Click(object sender, RoutedEventArgs e)
        {
            FocusManager.SetFocusedElement(this, this);
            Keyboard.ClearFocus();

            _main.GetGerenciarClientes();
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
        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is not DataGrid dg) return;

            // só segue se o double-click foi numa linha
            var depObj = e.OriginalSource as DependencyObject;
            var row = ItemsControl.ContainerFromElement(dg, depObj) as DataGridRow;
            if (row == null) return;

            if (DataContext is CalendarioViewModel vm)
            {
                // Se você já binda SelectedItem, pode usar o selecionado:
                var item = vm.AgendamentoSelecionado;
                if (item == null) return;

                vm.AbrirPagamentosAsync(item.Id);  // abre/preenche a modal
                vm.HistoricoCliente();               // abre/atualiza a coluna de histórico
                vm.AplicarDestaqueNoHistorico();     // garante o highlight no card certo
            }
        }

        private void Agendamento_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            var panel = (FrameworkElement)sender;
            var ag = (Agendamento)panel.DataContext;
            // inicia o drag com o próprio objeto Agendamento
            DragDrop.DoDragDrop(panel, ag, DragDropEffects.Move);
        }
        private void Dia_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Agendamento)))
            {
                e.Effects = DragDropEffects.Move;
                var border = (Border)sender;
                // cor provisória enquanto está arrastando por cima
                border.Background = Brushes.LightBlue;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }
        private void Agendamento_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not FrameworkElement fe) return;
            if (fe.DataContext is not Agendamento ag) return;
            if (DataContext is CalendarioViewModel vm)
                vm.SelecionarAgendamento(ag);
            e.Handled = true;
        }

        private void Dia_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(Agendamento))) return;

            var ag = (Agendamento)e.Data.GetData(typeof(Agendamento));
            // O DataContext do Border é o "ViewModel de dia" que expõe a propriedade Data (DateTime)
            var cellVm = (DiaCalendario)((FrameworkElement)sender).DataContext;
            DateTime novaData = cellVm.Data;

            // Dispara um comando na VM de calendário:
            var vm = (CalendarioViewModel)DataContext;
            vm.MoverAgendamentoCommand.Execute((ag, novaData));

            var border = (Border)sender;
            border.ClearValue(Border.BackgroundProperty);
        }
        private void Dia_DragLeave(object sender, DragEventArgs e)
        {
            var border = (Border)sender;
            border.ClearValue(Border.BackgroundProperty);
        }
    }
}
