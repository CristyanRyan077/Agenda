using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgendaNovo.Models;

namespace AgendaNovo.Interfaces
{
    public interface IPacoteService
    {
        List<Pacote> GetAll();
        Pacote? GetById(int id);
        void Add(Pacote pacote);
        void Update(Pacote pacote);
        void Delete(int id);
    }
}
