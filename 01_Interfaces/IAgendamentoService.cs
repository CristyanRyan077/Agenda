using AgendaNovo.Models;
using AgendaNovo.Services;
using AgendaNovo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgendaNovo.Interfaces
{
    public interface IAgendamentoService
    {
        List<Agendamento> GetAll();
        Agendamento? GetById(int id);
        Agendamento Add(Agendamento agendamento);
        void Update(Agendamento agendamento);
        void Delete(int id);

        void AtivarSePendente(int agendamentoid);
        void ValorIncompleto(int agendamentoid);


        // Filtros úteis
        List<Agendamento> GetByDate(DateTime data);
        List<Agendamento> GetByCliente(int clienteId);
        List<Agendamento> GetByCrianca(int criancaId);

        IQueryable<AgendaNovo.Services.FinanceiroRow> QueryFinanceiro(
           DateTime inicio, DateTime fim,
           int? servicoId = null,
           StatusAgendamento? status = null);


        Task AtualizarFotosAsync(int agendamentoId, FotosReveladas fotos);
        Task<FinanceiroResumo> CalcularKpisAsync(DateTime inicio, DateTime fim, int? servicoId = null, int? produtoId = null, StatusAgendamento? status = null);
        Task<List<RecebivelDTO>> ListarEmAbertoAsync(DateTime inicio, DateTime fim, int? servicoId = null, StatusAgendamento? status = null);
        Task<List<ServicoResumoDTO>> ResumoPorServicoAsync(DateTime inicio, DateTime fim, int? servicoId = null, StatusAgendamento? status = null);
        public Task<List<ProdutoResumoVM>> ResumoPorProdutoAsync(
        DateTime inicio, DateTime fim, int? produtoId = null, StatusAgendamento? status = null);
    }

}
