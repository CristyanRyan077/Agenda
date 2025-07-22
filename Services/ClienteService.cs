using AgendaNovo.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgendaNovo.Services
{
    public class ClienteService
    {
        private readonly AgendaContext _db;
        public event Action? DadosAlterados;
        public ObservableCollection<Cliente> Clientes { get; private set; } = new();

        public ClienteService(AgendaContext db)
        {
            _db = db;
        }

        private void NotificarAlteracoes()
        {
            DadosAlterados?.Invoke();
        }

        public List<Cliente> ObterTodos()
        {
            return _db.Clientes.Include(c => c.Criancas).ToList();
        }

        public Cliente? ObterPorId(int id)
        {
            return _db.Clientes.Include(c => c.Criancas).FirstOrDefault(c => c.Id == id);
        }

        public bool ClienteExiste(string nome, int? idIgnorar = null)
        {
            return _db.Clientes.Any(c => c.Nome == nome && (!idIgnorar.HasValue || c.Id != idIgnorar.Value));
        }

        public void Adicionar(Cliente cliente)
        {
            _db.Clientes.Add(cliente);
            _db.SaveChanges();
            NotificarAlteracoes();
        }

        public void Atualizar(Cliente cliente)
        {
            _db.Clientes.Update(cliente);
            _db.SaveChanges();
            NotificarAlteracoes();
        }
        public Cliente ObterComAgendamentos(int id)
        {
            return _db.Clientes
                .Include(c => c.Criancas)
                .Include(c => c.Agendamentos)
                .FirstOrDefault(c => c.Id == id);
        }
        public List<Agendamento> ObterAgendamentosDoCliente(int clienteId)
        {
            return _db.Agendamentos
                .Where(a => a.ClienteId == clienteId)
                .ToList();
        }

        public void Remover(int id)
        {
            var cliente = _db.Clientes
           .Include(c => c.Criancas)  // CARREGA AS CRIANÇAS
           .FirstOrDefault(c => c.Id == id);

            if (cliente != null)
            {
                _db.Criancas.RemoveRange(cliente.Criancas);
                _db.Clientes.Remove(cliente);
                _db.SaveChanges();
                NotificarAlteracoes();
            }
        }
    }
}
