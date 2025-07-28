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
    public class PacoteService : IPacoteService
    {
        private readonly AgendaContext _context;

        public PacoteService(AgendaContext context)
        {
            _context = context;
        }

        public List<Pacote> GetAll()
        {
            return _context.Pacotes
                .Include(p => p.Servico) // traz o nome do serviço junto
                .ToList();
        }

        public Pacote? GetById(int id)
        {
            return _context.Pacotes
                .Include(p => p.Servico)
                .FirstOrDefault(p => p.Id == id);
        }

        public void Add(Pacote pacote)
        {
            _context.Pacotes.Add(pacote);
            _context.SaveChanges();
        }

        public void Update(Pacote pacote)
        {
            _context.Pacotes.Update(pacote);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var pacote = _context.Pacotes.Find(id);
            if (pacote != null)
            {
                _context.Pacotes.Remove(pacote);
                _context.SaveChanges();
            }
        }
    }
}
