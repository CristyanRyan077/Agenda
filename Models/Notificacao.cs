using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgendaNovo.Models
{
    public class Notificacao
    {
        public int AgendamentoId { get; set; }
        public string NomeCliente { get; set; }
        public DateTime DataAgendamento { get; set; }
        public bool Enviada { get; set; }
    }
}
