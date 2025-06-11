using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Agenda
{
    public partial class frmLogin : Form
    {
        public frmLogin()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            Login();
        }

        private bool AutenticacaoLogin(string usuario, string senha)
        {
                return (string.Equals(usuario.Trim(), "admin", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(senha.Trim(), "cristyan653", StringComparison.OrdinalIgnoreCase));
        }

        private void Login()
        {
            if (AutenticacaoLogin(txtUsuario.Text, txtSenha.Text))
            {
                this.Close();
            }
            else
            {
                MessageBox.Show("Credenciais inválidas!", "Erro", MessageBoxButtons.OK);
            }
        }

        private void txtSenha_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Login();
            }
        }
    }
}
