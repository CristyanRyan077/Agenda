﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgendaNovo.Interfaces;
using AgendaNovo.Models;
using Microsoft.EntityFrameworkCore;

namespace AgendaNovo.Services
{
    public class ClienteService : IClienteService
    {
        private readonly AgendaContext _db;
        public ClienteService(AgendaContext db)
        {
            _db = db;
        }

        public List<Cliente> GetAll()
        {
            return _db.Clientes.AsNoTracking().ToList();
        }

        public List<Cliente> GetAllWithChildren()
        {
            return _db.Clientes
                .Include(c => c.Criancas)
                .AsNoTracking()
                .ToList();
        }

        public Cliente? GetById(int id)
        {
            return _db.Clientes
                .Include(c => c.Criancas)
                .FirstOrDefault(c => c.Id == id);
        }

        public Cliente? DetectExisting(string? telefone, string? email)
        {
            if (!string.IsNullOrWhiteSpace(telefone))
            {
                var byTel = _db.Clientes.FirstOrDefault(c => c.Telefone == telefone);
                if (byTel != null) return byTel;
            }
            if (!string.IsNullOrWhiteSpace(email))
            {
                var byEmail = _db.Clientes.FirstOrDefault(c => c.Email == email);
                return byEmail;
            }
            return null;
        }

        public Cliente Add(Cliente cliente)
        {
            _db.Clientes.Add(cliente);
            _db.SaveChanges();
            return cliente;
        }

        public void Update(Cliente cliente)
        {
            _db.Clientes.Update(cliente);
            _db.SaveChanges();
        }

        public void Delete(int id)
        {
            var cliente = _db.Clientes.Find(id);
            if (cliente == null) return;
            _db.Clientes.Remove(cliente);
            _db.SaveChanges();
        }
        public List<Agendamento> GetAgendamentos(int clienteId)
        {
            return _db.Agendamentos
                .Include(a => a.Crianca)
                .Include(a => a.Cliente)
                .Where(a => a.ClienteId == clienteId)
                .AsNoTracking()
                .ToList();
        }
    }
}
