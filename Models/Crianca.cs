using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgendaNovo.Models
{
    public class Crianca
    {
        public int Id { get; set; }
        public string ?Nome { get; set; }
        public string ?Idade { get; set; }
        public string ?Genero { get; set; }
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        public List<Agendamento> Agendamentos { get; set; } = new();
    }
}
