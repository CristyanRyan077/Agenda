using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgendaNovo.Interfaces;
using AgendaNovo.Models;
using Microsoft.EntityFrameworkCore;

namespace AgendaNovo.Services
{
    public class CriancaService : ICriancaService
    {
        private readonly AgendaContext _db;
        public CriancaService(AgendaContext db)
        {
            _db = db;
        }

        public List<Crianca> GetByClienteId(int clienteId)
        {
            return _db.Criancas
                .Where(c => c.ClienteId == clienteId)
                .AsNoTracking()
                .ToList();
        }

        public Crianca? GetById(int id)
        {
            return _db.Criancas.Find(id);
        }

        public Crianca AddOrUpdate(Crianca crianca)
        {
            if (crianca.Id == 0)
                _db.Criancas.Add(crianca);
            else
                _db.Criancas.Update(crianca);

            _db.SaveChanges();
            return crianca;
        }

        public void Delete(int id)
        {
            var crianca = _db.Criancas.Find(id);
            if (crianca == null) return;
            _db.Criancas.Remove(crianca);
            _db.SaveChanges();

        }
        public List<Agendamento> GetAgendamentos(int criancaId)
        {
            return _db.Agendamentos
                .Include(a => a.Cliente)
                .Include(a => a.Crianca)
                .Where(a => a.CriancaId == criancaId)
                .AsNoTracking()
                .ToList();
        }
    }

}
