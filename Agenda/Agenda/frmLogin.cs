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
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            Login();
        }

        private bool AutenticacaoLogin(string senha)
        {
             return (string.Equals(senha.Trim(), "cristyan653", StringComparison.OrdinalIgnoreCase));
        }

        private void Login()
        {
            if (AutenticacaoLogin(txtSenha.Text))
            {
                this.DialogResult = DialogResult.OK;
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
        public string Usuario
        {
            get { return txtUsuario.Text; }
        }
    }
}
