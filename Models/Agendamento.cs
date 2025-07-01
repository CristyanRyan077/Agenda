using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgendaNovo.Models;

namespace AgendaNovo
{
    public class Agendamento
    {
        public int Id { get; set; }

        public int ClienteId { get; set; }
        public Cliente ?Cliente { get; set; }
        public int CriancaId { get; set; }
        public Crianca? Crianca { get; set; }

        public string ?Servico { get; set; }
        public string ?Pacote { get; set; }
        public string ?Horario { get; set; }
        public string ?Tema { get; set; }
        public decimal Valor { get; set; }
        public decimal ValorPendente { get; set; }
        public DateTime Data { get; set; } = DateTime.Today;
    }
}
