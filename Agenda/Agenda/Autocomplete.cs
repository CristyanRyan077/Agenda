using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Agenda
{
    public static class Autocomplete
    {
        public static Dictionary<string, int> AutoComplete(TextBox textBox, DataTable dados,
            string campoTexto, string campoId)
        {
            var dicionario = dados.AsEnumerable()
                .ToDictionary(
                    row => $"{row[campoTexto]} (ID:{row[campoId]})",
                    row => Convert.ToInt32(row[campoId])
                );

            textBox.AutoCompleteCustomSource = new AutoCompleteStringCollection();
            textBox.AutoCompleteCustomSource.AddRange(dicionario.Keys.ToArray());
            textBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            textBox.AutoCompleteSource = AutoCompleteSource.CustomSource;

            return dicionario;
        }

        public static int? ObterIdSelecionado(string texto, Dictionary<string, int> dicionario)
        {
            var chave = dicionario.Keys
                .FirstOrDefault(k => k.StartsWith(texto, StringComparison.OrdinalIgnoreCase));
            return chave != null ? dicionario[chave] : (int?)null;
        }
    }
}
