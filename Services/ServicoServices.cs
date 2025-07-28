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
    public class ServicoService : IServicoService
    {
        private readonly AgendaContext _db;

        public ServicoService(AgendaContext db)
        {
            _db = db;
        }

        public List<Servico> GetAll()
        {
            return _db.Servicos.AsNoTracking().ToList();
        }

        public Servico? GetById(int id)
        {
            return _db.Servicos.Find(id);
        }

        public void Add(Servico servico)
        {
            _db.Servicos.Add(servico);
            _db.SaveChanges();
        }

        public void Update(Servico servico)
        {
            _db.Servicos.Update(servico);
            _db.SaveChanges();
        }

        public void Delete(int id)
        {
            var servico = _db.Servicos.Find(id);
            if (servico != null)
            {
                _db.Servicos.Remove(servico);
                _db.SaveChanges();
            }
        }
    }
}
