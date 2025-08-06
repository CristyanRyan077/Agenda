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
        public Calendario(CalendarioViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        private void Dia_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is DiaCalendario dia && DataContext is CalendarioViewModel vm)
            {
                vm.SelecionarDia(dia.Data);
            }
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
        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is CalendarioViewModel vm)
            {
                vm.EditarAgendamentoSelecionado();
            }
        }
    }
}
