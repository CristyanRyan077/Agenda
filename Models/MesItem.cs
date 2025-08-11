using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgendaNovo.Models
{
    public class MesItem
    {
        public int Numero { get; set; }
        public string? Nome { get; set; }

        public override string? ToString() => Nome; 
    }
}
