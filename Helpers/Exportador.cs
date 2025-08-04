using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgendaNovo.Models;
using ClosedXML.Excel;
using Microsoft.Win32;

namespace AgendaNovo.Helpers
{
    public static class Exportador
    {
        public static void ExportarClienteParaExcel(ClienteCriancaView cliente, List<Agendamento> agendamentos)
        {
            var salvar = new SaveFileDialog
            {
                FileName = $"Cliente_{cliente.NomeCliente}.xlsx",
                Filter = "Arquivo Excel (*.xlsx)|*.xlsx"
            };

            if (salvar.ShowDialog() != true)
                return;

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Cliente");

            // Dados do cliente
            ws.Cell("A1").Value = "Nome:";
            ws.Cell("B1").Value = cliente.NomeCliente;

            ws.Cell("B1").Value = "Telefone:";
            ws.Cell("B2").Value = cliente.Telefone;

            ws.Cell("C1").Value = "Email:";
            ws.Cell("B3").Value = cliente.Email;

            ws.Cell("D1").Value = "Status Cliente:";
            ws.Cell("B4").Value = cliente.Status.ToString();

            ws.Cell("1").Value = "Status Cliente:";

            ws.Cell("A5").Value = "Observação:";
            ws.Cell("B5").Value = cliente.Observacao;


            // Espaço e cabeçalho de agendamentos
            ws.Cell("A7").Value = "Data";
            ws.Cell("B7").Value = "Serviço";
            ws.Cell("C7").Value = "Status";

            int linha = 8;
            foreach (var ag in agendamentos)
            {
                ws.Cell(linha, 1).Value = ag.Data.ToString("dd/MM/yyyy");
                ws.Cell(linha, 2).Value = ag.Servico?.Nome ?? "";
                ws.Cell(linha, 3).Value = ag.Status.ToString();
                linha++;
            }

            ws.Columns().AdjustToContents(); // Auto ajuste de colunas
            workbook.SaveAs(salvar.FileName);
        }
    }
}
