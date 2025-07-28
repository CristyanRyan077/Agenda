using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgendaNovo.Models;

namespace AgendaNovo.Interfaces
{
    public interface IServicoService
    {
        List<Servico> GetAll();
        Servico? GetById(int id);
        void Add(Servico servico);
        void Update(Servico servico);
        void Delete(int id);
    }
}
