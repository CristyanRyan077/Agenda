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

namespace Agenda
{
    public partial class frmAgenda : Form
    {
        public frmAgenda()
        {
            InitializeComponent();


        }
        private void AdicionarComboBox(string header, string dataProperty,
            object dataSource, string displayMember, string valueMember, int width = 140)
        {
            DataGridViewComboBoxColumn comboCol = new DataGridViewComboBoxColumn();
            comboCol.HeaderText = header;
            comboCol.DataPropertyName = dataProperty;
            comboCol.DataSource = dataSource;
            comboCol.DisplayMember = displayMember;
            comboCol.ValueMember = valueMember;
            comboCol.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;
            comboCol.Width = width;
            agendamentoDataGridView.Columns.Add(comboCol);
        }
        private void agendamentoBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            try
            {
                this.Validate();
                this.agendamentoBindingSource.EndEdit();
                this.tableAdapterManager.UpdateAll(this.agendaDataSet);
                MessageBox.Show("Salvo com sucesso!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar: {ex.Message}");
            }
        }
        private void agendamentoDataGridView_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            MessageBox.Show("Preencha todos os campos obrigatórios antes de sair da linha.", "Erro de Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            e.Cancel = true;
        }

        private void frmAgenda_Load(object sender, EventArgs e)
        {
            agendamentoDataGridView.AllowUserToAddRows = false;
            agendamentoDataGridView.AutoGenerateColumns = false;
            agendamentoDataGridView.Columns.Clear();
            this.clienteTableAdapter1.Fill(this.agendaDataSet.Cliente);
            this.criancaTableAdapter1.Fill(this.agendaDataSet.Crianca);
            this.pacotesTableAdapter1.Fill(this.agendaDataSet.Pacotes);
            this.categoriaTableAdapter1.Fill(this.agendaDataSet.Categoria);
            this.agendamentoTableAdapter.Fill(this.agendaDataSet.Agendamento);


            AdicionarComboBox("Cliente", "ID_CLIENTE", agendaDataSet.Cliente, "NOME_CLIENTE", "ID_CLIENTE");
            AdicionarComboBox("Criança", "ID_CRIANCA", agendaDataSet.Crianca, "NOME", "ID_CRIANCA");
            AdicionarComboBox("Categoria", "ID_CATEGORIA", agendaDataSet.Categoria, "NOME_CATEGORIA", "ID_CATEGORIA");
            AdicionarComboBox("Pacotes", "ID_PACOTE", agendaDataSet.Pacotes, "NOME_PACOTE", "ID_PACOTE");

            AdicionarTextBox("Valor Total", "VALOR_TOTAL");
            AdicionarTextBox("Status", "STATUS");
            AdicionarTextBox("Tema", "TEMA");
            AdicionarDateTimeColumn("Horário", "HORARIO");


        }
        private void AdicionarTextBox(string header, string dataProperty)
        {
            var textCol = new DataGridViewTextBoxColumn();
            textCol.HeaderText = header;
            textCol.DataPropertyName = dataProperty;
            agendamentoDataGridView.Columns.Add(textCol);
        }

        private void AdicionarDateTimeColumn(string header, string dataProperty)
        {
            var col = new CalendarColumn();
            col.HeaderText = header;
            col.DataPropertyName = dataProperty;
            agendamentoDataGridView.Columns.Add(col);
        }

        private void agendamentoDataGridView_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (e.Control is ComboBox cb)
            {
                cb.DropDownStyle = ComboBoxStyle.DropDown; 
                cb.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                cb.AutoCompleteSource = AutoCompleteSource.ListItems;
            }
        }
    }
}
