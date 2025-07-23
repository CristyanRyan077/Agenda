using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgendaNovo.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgendaNovo.Services
{
    public class AgendamentoService : IAgendamentoService
    {
        private readonly AgendaContext _db;
        public AgendamentoService(AgendaContext db)
        {
            _db = db;
        }

        public List<Agendamento> GetAll()
        {
            return _db.Agendamentos
                .Include(a => a.Cliente)
                .Include(a => a.Crianca)
                .AsNoTracking()
                .ToList();
        }

        public Agendamento? GetById(int id)
        {
            return _db.Agendamentos
                .Include(a => a.Cliente)
                .Include(a => a.Crianca)
                .FirstOrDefault(a => a.Id == id);
        }

        public Agendamento Add(Agendamento agendamento)
        {
            _db.Agendamentos.Add(agendamento);
            _db.SaveChanges();
            return agendamento;
        }

        public void Update(Agendamento agendamento)
        {
            _db.Agendamentos.Update(agendamento);
            _db.SaveChanges();
        }

        public void Delete(int id)
        {
            var ag = _db.Agendamentos.Find(id);
            if (ag == null) return;
            _db.Agendamentos.Remove(ag);
            _db.SaveChanges();
        }

        public List<Agendamento> GetByDate(DateTime data)
        {
            return _db.Agendamentos
                .Include(a => a.Cliente)
                .Include(a => a.Crianca)
                .Where(a => a.Data.Date == data.Date)
                .AsNoTracking()
                .ToList();
        }

        public List<Agendamento> GetByCliente(int clienteId)
        {
            return _db.Agendamentos
                .Include(a => a.Cliente)
                .Include(a => a.Crianca)
                .Where(a => a.ClienteId == clienteId)
                .AsNoTracking()
                .ToList();
        }

        public List<Agendamento> GetByCrianca(int criancaId)
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
