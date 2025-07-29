using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgendaNovo.Interfaces
{
    public interface IAgendamentoService
    {
        List<Agendamento> GetAll();
        Agendamento? GetById(int id);
        Agendamento Add(Agendamento agendamento);
        void Update(Agendamento agendamento);
        void Delete(int id);

        void AtivarSePendente(int agendamentoid);

        // Filtros úteis
        List<Agendamento> GetByDate(DateTime data);
        List<Agendamento> GetByCliente(int clienteId);
        List<Agendamento> GetByCrianca(int criancaId);
    }

}
