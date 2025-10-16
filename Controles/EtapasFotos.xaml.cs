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
    /// Interação lógica para EtapasFotos.xam
    /// </summary>
    public partial class EtapasFotos : UserControl
    {
        public EtapasFotos()
        {
            InitializeComponent();
        }
        private void CopiarObsEscolha_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtObsEscolha.Text))
            {
                Clipboard.SetText(txtObsEscolha.Text);
            }
        }

    }
}
