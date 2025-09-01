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
                .Include(c => c.Agendamentos)
                    .ThenInclude(a => a.Pacote)
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
        public void AtivarSePendente(int clienteId)
        {
            var cliente = GetById(clienteId);
            if (cliente != null && (cliente.Status == StatusCliente.Pendente)
                || (cliente.Status == StatusCliente.Inativo)
                || (cliente.Status == StatusCliente.SA))
            {
                cliente.Status = StatusCliente.Ativo;
                Update(cliente);
            }
        }
        public void ValorIncompleto(int clienteId)
        {
            var cliente = GetById(clienteId);
            if (cliente != null && (cliente.Status == StatusCliente.Inativo)
                || (cliente.Status == StatusCliente.Ativo)
                || (cliente.Status == StatusCliente.SA))
            {
                cliente.Status = StatusCliente.Pendente;
                Update(cliente);
            }
        }
        public void ClienteInativo()
        {
            var agora = DateTime.Now;
            var clientes = _db.Clientes
                .Include(c => c.Agendamentos)
                .ToList();

            var clientesInativados = new List<string>();

            foreach (var cliente in clientes)
            {
                if (cliente.Agendamentos.Any())
                {
                    var ultimaData = cliente.Agendamentos.Max(a => a.Data);

                    if (ultimaData < agora.AddDays(-60) &&
                        (cliente.Status == StatusCliente.Ativo || cliente.Status == StatusCliente.Pendente))
                    {
                        cliente.Status = StatusCliente.Inativo;
                        _db.Update(cliente);
                        var diasInativos = (agora - ultimaData).Days;
                        clientesInativados.Add($"{cliente.Nome} - Tel: {cliente.Telefone} - Inativo há {diasInativos} dias");
                    }
                }
            }

            _db.SaveChanges();
            if (clientesInativados.Any())
            {
                var mensagem = "Clientes marcados como inativos:\n\n" + string.Join("\n", clientesInativados);
                MessageBox.Show(mensagem, "Clientes Inativos", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        public List<Agendamento> GetAgendamentos(int clienteId)
        {
            return _db.Agendamentos
                .Include(a => a.Crianca)
                .Include(a => a.Cliente)
                .Include(a => a.Pacote)
                .Include(a => a.Servico)
                .Include(a => a.Pagamentos)
                .Where(a => a.ClienteId == clienteId)
                .AsNoTracking()
                .ToList();
        }
    }
}
