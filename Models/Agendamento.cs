using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgendaNovo
{
    public class Agendamento
    {
        public int Id { get; set; }
        public string ?Cliente { get; set; }
        public string ?Pacote { get; set; }
        public string ?Horario { get; set; }
        public DateTime Data { get; set; } = DateTime.Today;
    }
}
