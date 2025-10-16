using AgendaNovo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AgendaNovo.AgendaViewModel;

namespace AgendaNovo.Interfaces
{
    public interface IProdutoService
    {
        List<Produto> GetAll();
        Produto? GetById(int id);
        void Add(Produto produto);
        void Update(Produto produto);
        void Delete(int id);
    }
}
