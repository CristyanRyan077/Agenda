using AgendaNovo.Dtos;
using AgendaNovo.Models;
using AgendaNovo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AgendaNovo.Services.PagamentoService;

namespace AgendaNovo.Interfaces
{
    public interface IPagamentoService
    {
        Task<ResumoAgendamentoDto> ObterResumoAgendamentoAsync(int agendamentoId);
        Task AdicionarPagamentoAsync(int agendamentoId, CriarPagamentoDto dto);
        Task AtualizarPagamentoAsync(AtualizarPagamentoDto dto);
        Task RemoverPagamentoAsync(int pagamentoID);
        Task<List<HistoricoFinanceiroDto>> ListarHistoricoAsync(int agendamentoId);
        Task AdicionarProdutoAoAgendamentoAsync(int agendamentoId,
            CriarProdutoAgendamentoDto dto,
            MetodoPagamento? metodo = null,
            string? observacao = null,
            DateTime? dataPagamento = null);
        Task RemoverProdutoDoAgendamentoAsync(int agendamentoProdutoId);
        //Task<bool> CriarTransacaoAsync(TransacaoDto dto);
    }
}
