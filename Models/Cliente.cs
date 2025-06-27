using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgendaNovo.Models;


namespace AgendaNovo.Models
{
    public class Cliente
    {
        public int Id { get; set; }
        public string ?Nome { get; set; }
        public string Crianca { get; set; }
        public string Telefone { get; set; }

        public List<Agendamento> Agendamentos { get; set; } = new();
    }
}
