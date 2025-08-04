using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AgendaNovo.Models
{
    public partial class Crianca : ObservableObject
    {

        public int Id { get; set; }
        [ObservableProperty] private string? nome;


        [ObservableProperty] private Genero genero = Genero.M;

        [ObservableProperty] private int? idade;

        [ObservableProperty] private IdadeUnidade idadeUnidade = IdadeUnidade.Meses;

        [ObservableProperty] private DateOnly? nascimento;

        [NotMapped]
        public DateTime? NascimentoDateTime
        {
            get => Nascimento?.ToDateTime(TimeOnly.MinValue);
            set => Nascimento = value.HasValue ? DateOnly.FromDateTime(value.Value) : null;
        }
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        public string? IdadeFormatada => $"{Idade} {IdadeUnidade}";
        public DateTime? UltimaAtualizacaoIdade { get; set; }
        partial void OnNascimentoChanged(DateOnly? value)
        {
            if (value.HasValue)
            {
                var hoje = DateOnly.FromDateTime(DateTime.Today);
                var nascimento = value.Value;

                var anos = hoje.Year - nascimento.Year;
                if (nascimento > hoje.AddYears(-anos)) anos--;

                if (anos > 0)
                {
                    Idade = anos;
                    IdadeUnidade = IdadeUnidade.Anos;
                }
                else
                {
                    var meses = (hoje.Year - nascimento.Year) * 12 + hoje.Month - nascimento.Month;
                    if (nascimento.Day > hoje.Day) meses--;

                    Idade = meses;
                    IdadeUnidade = IdadeUnidade.Meses;
                }
            }
        }

    }
}
