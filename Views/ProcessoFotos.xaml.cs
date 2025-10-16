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
using System.Windows.Shapes;

namespace AgendaNovo.Views
{
    /// <summary>
    /// Lógica interna para ProcessoFotos.xaml
    /// </summary>
    public partial class ProcessoFotos : Window
    {
        public ProcessoFotos()
        {
            InitializeComponent();
            Loaded += async (_, __) =>
            {
                if (DataContext is FotosViewModel vm)
                    await vm.CarregarAsync();
            };
        }

        private void DataGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not DataGrid dg) return;
            if (dg.SelectedItem is not FotoProcessoVM vm) return;

            // Chama o comando na VM:
            if (DataContext is FotosViewModel ctx && ctx.AbrirEtapaPelaLinhaCommand.CanExecute(vm))
                ctx.AbrirEtapaPelaLinhaCommand.Execute(vm);
        }
    }
}
