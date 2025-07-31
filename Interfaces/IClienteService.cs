using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgendaNovo.Models;

namespace AgendaNovo.Interfaces
{
    public interface IClienteService
    {
        List<Cliente> GetAll();
        List<Cliente> GetAllWithChildren();
        Cliente? GetById(int id);
        Cliente? DetectExisting(string? telefone, string? email);
        Cliente Add(Cliente cliente);


        void AtivarSePendente(int clienteId);
        void ValorIncompleto(int clienteId);
        void ClienteInativo();

        void Update(Cliente cliente);
        void Delete(int id);
        List<Agendamento> GetAgendamentos(int clienteId);
    }

}
