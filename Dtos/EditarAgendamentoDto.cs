using AgendaNovo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgendaNovo.Dtos
{
    public sealed class EditarAgendamentoDto
    {
        public Agendamento Agendamento { get; set; }
        public Cliente Cliente { get; set; }
        public List<Servico> Servicos { get; set; }
        public List<Pacote> Pacotes { get; set; }
        public List<Pacote> PacotesFiltrados { get; set; }
    }
}
