using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Agenda.AgendaDataSetTableAdapters;
using static Agenda.AgendaDataSet;

namespace Agenda
{
    public partial class frmClientes : Form
    {
        private Dictionary<string, int> _clientes;
        public frmClientes()
        {

            InitializeComponent();
            _clientes = clienteTableAdapter.GetData()
                .ToDictionary(c => $"{c.NOME_CLIENTE} (ID:{c.ID_CLIENTE})", c => c.ID_CLIENTE);

            txtCliente.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            txtCliente.AutoCompleteSource = AutoCompleteSource.CustomSource;
            txtCliente.AutoCompleteCustomSource = new AutoCompleteStringCollection();
            txtCliente.AutoCompleteCustomSource.AddRange(_clientes.Keys.ToArray());
        }

        private void clienteBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            this.Validate();
            this.clienteBindingSource.EndEdit();
            this.tableAdapterManager.UpdateAll(this.agendaDataSet);

        }

        private void frmClientes_Load(object sender, EventArgs e)
        {
            // TODO: esta linha de código carrega dados na tabela 'agendaDataSet.Cliente'. Você pode movê-la ou removê-la conforme necessário.
            this.clienteTableAdapter.Fill(this.agendaDataSet.Cliente);
            // TODO: esta linha de código carrega dados na tabela 'agendaDataSet.Crianca'. Você pode movê-la ou removê-la conforme necessário.
            this.criancaTableAdapter.Fill(this.agendaDataSet.Crianca);
            Responsavel();

        }

        private void Responsavel()
        {
            DataGridViewComboBoxColumn responsavelCol = new DataGridViewComboBoxColumn();
            responsavelCol.Name = "ResponsavelColumn";
            responsavelCol.HeaderText = "Responsável";
            responsavelCol.DataPropertyName = "ID_CLIENTE";  // Nome da FK na tabela principal
            responsavelCol.DataSource = this.agendaDataSet.Cliente;
            responsavelCol.DisplayMember = "NOME_CLIENTE";
            responsavelCol.ValueMember = "ID_CLIENTE";
            responsavelCol.ReadOnly = true;
            responsavelCol.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;
            criancaDataGridView.Columns.Add(responsavelCol);
        }

        private void criancaBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            if (!_clientes.TryGetValue(txtCliente.Text, out int clienteID))
            {
                MessageBox.Show("Cliente não encontrado!");
                return;
            }
            if (!int.TryParse(iDADETextBox.Text, out int idade))
            {
                MessageBox.Show("Idade deve ser um número válido!");
                return;
            }
            criancaTableAdapter.Insert(clienteID, nOMETextBox.Text,
            gENEROComboBox.Text, dATA_NASCIMENTODateTimePicker.Value, idade);
            this.criancaTableAdapter.Fill(this.agendaDataSet.Crianca);
            this.Validate();
            this.criancaBindingSource.EndEdit();
            this.tableAdapterManager.UpdateAll(this.agendaDataSet);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            criancaDataGridView.DataSource = clienteTableAdapter.GetData()
                .Where(c => c.NOME_CLIENTE.Contains(txtCliente.Text))
                .Take(100)
                .ToList();
        }
    }
}
