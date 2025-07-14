using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgendaNovo.ViewModels
{
    public class FormularioViewModel
    {
        private readonly AgendaContext _db;

        public FormularioViewModel(AgendaContext db)
        {
            _db = db;
        }
    }
}
