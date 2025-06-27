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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AgendaNovo
{
    /// <summary>
    /// Lógica interna para Agendar.xaml
    /// </summary>
        public partial class Agendar : Window
        {

            public Agendar(AgendaViewModel vm)
            {
                InitializeComponent();
                this.DataContext = vm;
            }


        private void btnAgendar_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
