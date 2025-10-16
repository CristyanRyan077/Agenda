using AgendaNovo.ViewModels;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgendaNovo.Helpers
{
    public static class WordExportHelper
    {
        public static void GenerateFotosResumoDocx(IEnumerable<FotoProcessoVM> rows, string filePath, string titulo = "Entrega das Fotos")
        {
            if (rows == null) throw new ArgumentNullException(nameof(rows));
            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using (var doc = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
            {
                var mainPart = doc.AddMainDocumentPart();
                mainPart.Document = new Document();
                var body = new Body();

                // Título
                body.Append(new Paragraph(
                    new ParagraphProperties(
                        new Justification { Val = JustificationValues.Center },
                        new SpacingBetweenLines { After = "200" }),
                    new Run(new RunProperties(new Bold(), new FontSize { Val = "28" }), new Text(titulo))));

                // Tabela
                var table = new Table();

                // Estilo da tabela (bordas)
                var props = new TableProperties(
                    new TableBorders(
                        new TopBorder { Val = BorderValues.Single, Size = 8 },
                        new BottomBorder { Val = BorderValues.Single, Size = 8 },
                        new LeftBorder { Val = BorderValues.Single, Size = 8 },
                        new RightBorder { Val = BorderValues.Single, Size = 8 },
                        new InsideHorizontalBorder { Val = BorderValues.Single, Size = 6 },
                        new InsideVerticalBorder { Val = BorderValues.Single, Size = 6 }
                    ),
                    new TableWidth { Type = TableWidthUnitValues.Pct, Width = "5000" } // 100%
                );
                table.AppendChild(props);

                // Cabeçalho
                var header = new[] { "Data", "Cliente", "Crianca", "Telefone", "Mesversario", "Serviço", "Etapa Atual", "Escolha", "Tratamento", "Revelar", "Entrega", "Status" };
                table.Append(CreateHeaderRow(header));

                // Linhas
                foreach (var r in rows)
                {
                    table.Append(CreateRow(new[]
                    {
                    FmtDate(r?.Data, "dd/MM/yyyy"),
                    r?.Cliente ?? "",
                    r?.Crianca ?? "",
                    r?.Telefone ?? "",
                    r?.MesversarioFormatado ?? "",
                    r?.Servico ?? "",
                    r?.EtapaAtual ?? "",
                    FmtDate(r?.EscolhaData, "dd/MM"),
                    FmtDate(r?.TratamentoData, "dd/MM"),
                    FmtDate(r?.RevelarData, "dd/MM"),
                    FmtDate(r?.EntregaData, "dd/MM"),
                    r?.Status.ToString() ?? ""
                }));
                }

                body.Append(table);

                // Seção: paisagem + margens
                body.Append(new SectionProperties(
                    new PageSize { Orient = PageOrientationValues.Landscape, Width = 16840, Height = 11900 }, // A4 landscape
                    new PageMargin { Top = 720, Bottom = 720, Left = 720, Right = 720 } // ~2,54 cm
                ));

                mainPart.Document.Append(body);
                mainPart.Document.Save();
            }

            static string FmtDate(DateTime? dt, string format) =>
                dt.HasValue && dt.Value > DateTime.MinValue ? dt.Value.ToString(format, CultureInfo.GetCultureInfo("pt-BR")) : "";

            static TableRow CreateHeaderRow(IEnumerable<string> cells)
            {
                var tr = new TableRow();
                foreach (var c in cells)
                    tr.Append(CreateCell(c, bold: true));
                return tr;
            }

            static TableRow CreateRow(IEnumerable<string> cells)
            {
                var tr = new TableRow();
                foreach (var c in cells)
                    tr.Append(CreateCell(c));
                return tr;
            }

            static TableCell CreateCell(string text, bool bold = false)
            {
                var runProps = new RunProperties();
                if (bold) runProps.Append(new Bold());

                var p = new Paragraph(
                    new ParagraphProperties(new SpacingBetweenLines { After = "0", Before = "0" }),
                    new Run(runProps, new Text(text ?? "") { Space = SpaceProcessingModeValues.Preserve })
                );

                var tc = new TableCell(
                    new TableCellProperties(new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Center }),
                    p
                );
                return tc;
            }
        }
    }
}
