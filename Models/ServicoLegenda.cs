using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AgendaNovo.Models
{
    public record ServicoLegenda(int Id, string Nome, Brush Cor);
    public static class ServicoPalette
    {
        private static Brush Hex(string hex) =>
            (SolidColorBrush)new BrushConverter().ConvertFromString(hex);

        // Ajuste nomes/cores conforme sua lista
        public static readonly IReadOnlyList<ServicoLegenda> All = new List<ServicoLegenda>
    {
        new(1,  "Smash The Cake",         Hex("#F44336")), // vermelho
        new(2,  "Acomp. Mensal",          Hex("#0288D1")), // azul
        new(3,  "Gestante",               Hex("#FB8C00")), // laranja
        new(4,  "Infantil",               Hex("#1E88E5")), // azul vivo
        new(5,  "Aniversário",            Hex("#8E24AA")), // roxo
        new(6,  "Casamento Civil",        Hex("#6D4C41")), // marrom
        new(7,  "Casamentos",             Hex("#AD1457")), // rosa escuro
        new(8,  "B-Day Adulto",           Hex("#3949AB")), // azul arroxeado
        new(9,  "Casal",                  Hex("#FBC02D")), // amarelo
        new(10, "Família",                Hex("#00796B")), // teal escuro
        new(11, "Ensaio Infantil",        Hex("#F57C00")), // laranja forte
        new(12, "Chá Revelação + Vídeo",  Hex("#5D4037")), // marrom médio
        new(13, "Pack de Fotos",          Hex("#2E7D32")), // verde
        new(14, "Corporativos",           Hex("#C2185B")), // rosa
        new(15, "Evento Religioso",       Hex("#7B1FA2")), // roxo escuro
        new(16, "Evento 15 Anos",         Hex("#558B2F")), // verde oliva
        new(17, "Book Niver",             Hex("#9E9D24")), // verde amarelado
    };

        public static Brush FromId(int id) =>
            All.FirstOrDefault(x => x.Id == id)?.Cor ?? Brushes.SteelBlue;

        public static string NameFromId(int id) =>
            All.FirstOrDefault(x => x.Id == id)?.Nome ?? "Serviço";
    }
}
