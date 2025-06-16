using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;

namespace Agenda
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
            using (frmLogin frmLogin = new frmLogin())
            {
                if (frmLogin.ShowDialog() == DialogResult.OK)
                {
                    lblHello.Text = ($"Seja bem vindo {frmLogin.Usuario}");
                }
            }
        }
        private void tsbClientes_Click(object sender, EventArgs e)
        {
            frmClientes novoCliente = new frmClientes();
            novoCliente.ShowDialog();

        }

        private void agendamentoBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            this.Validate();
            this.agendamentoBindingSource.EndEdit();

        }

        private void Main_Load(object sender, EventArgs e)
        {
            // TODO: esta linha de código carrega dados na tabela 'agendaDataSet.ConsultaAgendamento'. Você pode movê-la ou removê-la conforme necessário.
            this.consultaAgendamentoTableAdapter.Fill(this.agendaDataSet.ConsultaAgendamento);


        }

        private void tsbAgenda_Click(object sender, EventArgs e)
        {
            this.agendamentoTableAdapter.Fill(this.agendaDataSet.Agendamento);
            frmAgenda frmAgenda = new frmAgenda();
            frmAgenda.ShowDialog();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {

        }

        private void btnPesquisar_Click(object sender, EventArgs e)
        {
            DateTime dataInicio = dtp1.Value.Date;
            DateTime dataFim = dtp2.Value.Date.AddDays(1).AddSeconds(-1);

            try
            {
                this.consultaAgendamentoTableAdapter.FillAgenda(this.agendaDataSet.ConsultaAgendamento, dataInicio, dataFim);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao pesquisar: {ex.Message}");
            }
        }
    }
}
