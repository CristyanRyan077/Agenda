using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AgendaNovo.Models
{
    public class Servico
    {
        public int Id { get; set; }

        public string? Nome { get; set; }

        public bool PossuiCrianca { get; set; } = true;
        public Servico()
        {
            PossuiCrianca = true;
        }
    }
}
