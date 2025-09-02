using AgendaNovo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgendaNovo.Interfaces
{
    public interface IPagamentoService
    {
        Task<ResumoAgendamentoDto> ObterResumoAgendamentoAsync(int agendamentoId);
        Task<List<PagamentoDto>> ListarPagamentosAsync(int agendamentoId);
        Task AdicionarPagamentoAsync(int agendamentoId, CriarPagamentoDto dto);
        Task AtualizarPagamentoAsync(AtualizarPagamentoDto dto);
        Task RemoverPagamentoAsync(int pagamentoID);
    }
}
