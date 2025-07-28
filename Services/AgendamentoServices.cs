using AgendaNovo.Interfaces;
using AgendaNovo.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
                .Include (a => a.Pacote)
                .Include (a => a.Servico)
                .AsNoTracking()
                .ToList();
        }

        public Agendamento? GetById(int id)
        {
            return _db.Agendamentos
                .Include(a => a.Cliente)
                .Include(a => a.Crianca)
                .Include(a => a.Pacote)
                .Include(a => a.Servico)
                .FirstOrDefault(a => a.Id == id);
        }

        public Agendamento Add(Agendamento agendamento)
        {
            _db.Agendamentos.Add(agendamento);
            _db.SaveChanges();
            return agendamento;
        }
        private void CopiarDados(Agendamento origem, Agendamento destino)
        {
            destino.Data = origem.Data;
            destino.Horario = origem.Horario;
            destino.Tema = origem.Tema;
            destino.Valor = origem.Valor;
            destino.ValorPago = origem.ValorPago;
            destino.PacoteId = origem.PacoteId;
            destino.ServicoId = origem.ServicoId;
            destino.ClienteId = origem.ClienteId;
            destino.CriancaId = origem.CriancaId;
        }

        public void Update(Agendamento agendamento)
        {
            var existente = _db.Agendamentos
            .Include(a => a.Cliente)
            .Include(a => a.Crianca)
            .Include(a => a.Pacote)
            .Include(a => a.Servico)
            .FirstOrDefault(a => a.Id == agendamento.Id);
            if (existente != null)
            {
                CopiarDados(agendamento, existente);
                _db.SaveChanges();
            }
            else
                MessageBox.Show("agendamento inexistente");
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
                .Include(a => a.Pacote)
                .Include(a => a.Servico)
                .Where(a => a.Data.Date == data.Date)
                .AsNoTracking()
                .ToList();
        }

        public List<Agendamento> GetByCliente(int clienteId)
        {
            return _db.Agendamentos
                .Include(a => a.Cliente)
                .Include(a => a.Crianca)
                .Include(a => a.Pacote)
                .Include(a => a.Servico)
                .Where(a => a.ClienteId == clienteId)
                .AsNoTracking()
                .ToList();
        }

        public List<Agendamento> GetByCrianca(int criancaId)
        {
            return _db.Agendamentos
                .Include(a => a.Cliente)
                .Include(a => a.Crianca)
                .Include(a => a.Pacote)
                .Include(a => a.Servico)
                .Where(a => a.CriancaId == criancaId)
                .AsNoTracking()
                .ToList();
        }
    }
}
