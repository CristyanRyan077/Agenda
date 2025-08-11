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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AgendaNovo.Controles
{
    /// <summary>
    /// Interação lógica para EditarAgendamentoView.xam
    /// </summary>
    public partial class EditarAgendamentoView : UserControl
    {
        public event EventHandler? FecharSolicitado;
        public EditarAgendamentoView()
        {
            InitializeComponent();
        }
        private void OnFechar()
        {
            FecharSolicitado?.Invoke(this, EventArgs.Empty);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OnFechar();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            OnFechar();
        }
    }
}
