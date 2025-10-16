using AgendaNovo.Converters;
using AgendaNovo.Models;
using AgendaNovo.Services;
using AgendaNovo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AgendaNovo.Agendamento;

namespace AgendaNovo.Interfaces
{
    public interface IAgendamentoService
    {
        List<Agendamento> GetAll();
        Agendamento? GetById(int id);
        Agendamento? GetByIdAsNoTracking(int id);
        Agendamento Add(Agendamento agendamento);
        void Update(Agendamento agendamento);
        void Delete(int id);
        void AtivarSePendente(int agendamentoid);
        void ValorIncompleto(int agendamentoid);
        void UpdateStatus(int id, StatusAgendamento novoStatus);
        void UpdateItens(int agendamentoId, List<AgendamentoProduto> itens);
        Task ReagendarAsync(int agendamentoId, DateTime novaData, TimeSpan? novoHorario);
        AgendamentoEtapa AddOrUpdateEtapa(int agendamentoId, EtapaFotos etapa, DateTime data, string? obs);
        List<AgendamentoAtrasoDTO> GetAgendamentosComFotosAtrasadas(DateTime hoje);


        // Filtros úteis
        List<Agendamento> GetByDate(DateTime data);
        List<Agendamento> GetByCliente(int clienteId);
        List<Agendamento> GetByCrianca(int criancaId);

        IQueryable<AgendaNovo.Services.FinanceiroRow> QueryFinanceiro(
           DateTime inicio, DateTime fim,
           int? servicoId = null,
           StatusAgendamento? status = null, string? clienteNome = null);


        Task<FinanceiroResumo> CalcularKpisAsync(DateTime inicio, DateTime fim, int? servicoId = null, int? produtoId = null, StatusAgendamento? status = null, string? clienteNome = null);
        Task<List<RecebivelDTO>> ListarEmAbertoAsync(DateTime inicio, DateTime fim, int? servicoId = null, StatusAgendamento? status = null, string? clienteNome = null);
        Task<List<ServicoResumoDTO>> ResumoPorServicoAsync(DateTime inicio, DateTime fim, int? servicoId = null, StatusAgendamento? status = null, string? clienteNome = null);
        public Task<List<ProdutoResumoVM>> ResumoPorProdutoAsync(
        DateTime inicio, DateTime fim, int? produtoId = null, StatusAgendamento? status = null, string? clienteNome = null);
        Task<List<FotoProcessoVM>> ListarProcessoFotosAsync(DateTime inicio, DateTime fim, EtapaStatus? status = null, string? clienteNome = null);
        bool AbrirEtapaDialog(SetEtapaParam p);
    }

}
