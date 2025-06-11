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
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
            lbl_agenda.Click += (Sender, e) => pnlAgenda_Click(Sender, e);
            pbAgenda.Click += (Sender, e) => pnlAgenda_Click(Sender, e);

            lbl_agenda.MouseEnter += (Sender, e) => pnlAgenda_MouseEnter(Sender, e);
            pbAgenda.MouseEnter += (Sender, e) => pnlAgenda_MouseEnter(Sender, e);

            lbl_agenda.MouseLeave += (Sender, e) => pnlAgenda_MouseLeave(Sender, e);
            pbAgenda.MouseLeave += (Sender, e) => pnlAgenda_MouseLeave(Sender, e);
        }
        private void pnlAgenda_Click(object sender, EventArgs e)
        {
            MessageBox.Show("teste");
        }

        private void pnlAgenda_MouseEnter(object sender, EventArgs e)
        {
            pnlAgenda.BorderStyle = BorderStyle.Fixed3D;
            pnlAgenda.BackColor = Color.FromArgb(340, 240, 240);
        }

        private void pnlAgenda_MouseLeave(object sender, EventArgs e)
        {
            pnlAgenda.BorderStyle = BorderStyle.None;
            pnlAgenda.BackColor = SystemColors.Control;
        }
    }
}
