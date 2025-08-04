using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AgendaNovo.Models
{
    public class Servico
    {
        public int Id { get; set; }

        public string? Nome { get; set; }

        public bool PossuiCrianca { get; set; } = true;

        [NotMapped]
        public Brush Cor { get; set; } = Brushes.LightBlue;
        public Servico()
        {
            PossuiCrianca = true;
        }
    }
}
