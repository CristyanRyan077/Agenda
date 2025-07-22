using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgendaNovo.Services
{
    public class AgendaService
    {
        private readonly AgendaContext _db;

        public AgendaService(AgendaContext db)
        {
            _db = db;
        }

        public List<Agendamento> ObterTodos()
        {
            return _db.Agendamentos
                      .Include(a => a.Cliente)
                      .Include(a => a.Crianca)
                      .ToList();
        }

        public Agendamento? ObterPorId(int id)
        {
            return _db.Agendamentos
                      .Include(a => a.Cliente)
                      .Include(a => a.Crianca)
                      .FirstOrDefault(a => a.Id == id);
        }


        public void Adicionar(Agendamento agendamento)
        {
            // Toda a validação que estava na ViewModel
            var cliente = _db.Clientes.Include(c => c.Criancas)
                            .FirstOrDefault(c => c.Id == agendamento.ClienteId);

            if (cliente == null)
                throw new Exception("Cliente não encontrado");

            if (!cliente.Criancas.Any(c => c.Id == agendamento.CriancaId))
                throw new Exception("Criança não pertence ao cliente");

            if (TemAgendamentosNoDia(agendamento.Data, agendamento.Horario))
                throw new Exception("Horário já ocupado");

            _db.Agendamentos.Add(agendamento);
            _db.SaveChanges();
            NotificarAlteracoes();
        }

        public void Atualizar(Agendamento agendamento)
        {
            _db.Agendamentos.Update(agendamento);
            _db.SaveChanges();
        }

        public void Remover(int id)
        {
            var agendamento = _db.Agendamentos.Find(id);
            if (agendamento != null)
            {
                _db.Agendamentos.Remove(agendamento);
                _db.SaveChanges();
            }
        }

        public List<Agendamento> ObterPorData(DateTime data)
        {
            return _db.Agendamentos
                      .Include(a => a.Cliente)
                      .Include(a => a.Crianca)
                      .Where(a => a.Data.Date == data.Date)
                      .ToList();
        }
        public bool TemAgendamentosNoDia(DateTime data, string horario)
        {
            return _db.Agendamentos.Any(a => a.Data.Date == data.Date && a.Horario == horario);
        }
    }
}
