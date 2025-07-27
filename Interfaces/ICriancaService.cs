using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgendaNovo.Models;

namespace AgendaNovo.Interfaces
{
        public interface ICriancaService
        {
            List<Crianca> GetByClienteId(int clienteId);
            Crianca? GetById(int id);
            Crianca AddOrUpdate(Crianca crianca);
            void Delete(int id);
            void AtualizarIdadeDeTodasCriancas();
            List<Agendamento> GetAgendamentos(int criancaId);
    }
}
