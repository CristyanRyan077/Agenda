using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgendaNovo.Models
{
    public class Produto
    {
        public int Id { get; set; }
        [MaxLength (50)]
        public string Nome { get; set; } = "";

        public decimal Valor { get; set; }
        public int PrazoProducaoDias { get; set; } = 15;
    }
}
