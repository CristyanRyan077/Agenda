using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgendaNovo.Interfaces
{
    public interface INotificacaoService
    {
        void VerificarAgendamentos();
        List<Agendamento> GetAgendamentosParaAmanha();
    }
}
