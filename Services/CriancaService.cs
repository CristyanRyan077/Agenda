using AgendaNovo.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgendaNovo.Services
{
    public class CriancaService
    {
        private readonly AgendaContext _db;

        public CriancaService(AgendaContext db)
        {
            _db = db;
        }

        public List<Crianca> ObterTodos()
        {
            return _db.Criancas.Include(c => c.Cliente).ToList();
        }

        public Crianca? ObterPorId(int id)
        {
            return _db.Criancas.Include(c => c.Cliente).FirstOrDefault(c => c.Id == id);
        }

        public bool CriancaExiste(string nome, int? idIgnorar = null)
        {
            return _db.Criancas.Any(c => c.Nome == nome && (!idIgnorar.HasValue || c.Id != idIgnorar.Value));
        }

        public void Adicionar(Crianca crianca)
        {
            _db.Criancas.Add(crianca);
            _db.SaveChanges();
        }

        public void Atualizar(Crianca crianca)
        {
            _db.Criancas.Update(crianca);
            _db.SaveChanges();
        }

        public void Remover(int id)
        {
            var crianca = _db.Criancas.Find(id);
            if (crianca != null)
            {
                _db.Criancas.Remove(crianca);
                _db.SaveChanges();
            }
        }
        public void RemoverRange(IEnumerable<Crianca> criancas)
        {
            _db.Criancas.RemoveRange(criancas);
            _db.SaveChanges();
        }
        public List<Crianca> ObterPorClienteId(int clienteId)
        {
            return _db.Criancas.Where(c => c.ClienteId == clienteId).ToList();
        }
    }
}
