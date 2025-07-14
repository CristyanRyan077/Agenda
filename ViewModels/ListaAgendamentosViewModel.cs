using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgendaNovo.ViewModels
{
    public class ListaAgendamentosViewModel
    {
        private readonly AgendaContext _db;

        public ListaAgendamentosViewModel(AgendaContext db)
        {
            _db = db;
        }
    }
}
