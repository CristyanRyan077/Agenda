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

namespace AgendaNovo.Controles
{
    /// <summary>
    /// Lógica interna para EtapaDialogView.xaml
    /// </summary>
    public partial class EtapaDialogView : Window
    {
        public EtapaDialogView()
        {
            InitializeComponent();
        }
        private void Salvar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
