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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AgendaNovo.Controles
{
    /// <summary>
    /// Interação lógica para HistoricoUsuario.xam
    /// </summary>
    public partial class HistoricoUsuario : UserControl
    {
        public HistoricoUsuario()
        {
            InitializeComponent();
        }
        private void HistoricoItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is AgendamentoHistoricoVM item
                && DataContext is CalendarioViewModel vm)
            {
                if (vm.PagamentosAgendamentoCommand.CanExecute(item.Agendamento))
                    vm.PagamentosAgendamentoCommand.Execute(item.Agendamento);
            }
        }
    }
}
