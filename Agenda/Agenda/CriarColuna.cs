using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Agenda
{
    public static class GridHelper 
    {
        public static DataGridViewComboBoxColumn CriarColunaFK(
            string nomeColuna,
            string headerText,
            string dataPropertyName,
            DataTable dadosFonte,
            string displayMember,
            string valueMember)
        {
            return new DataGridViewComboBoxColumn
            {
                Name = nomeColuna,
                HeaderText = headerText,
                DataPropertyName = dataPropertyName,
                DataSource = dadosFonte,
                DisplayMember = displayMember,
                ValueMember = valueMember,
                DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing,
                FlatStyle = FlatStyle.Flat,
                Width = 180,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.WhiteSmoke,
                    SelectionBackColor = Color.LightSteelBlue
                }
            };
        }
    }
}
