using AgendaNovo.Dtos;
using AgendaNovo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgendaNovo._01_Interfaces
{
    public interface IAcoesService
    {
        // 1) Carrega tudo que a tela de edição precisa
        Task<EditarAgendamentoDto?> PrepararEdicaoAsync(int agendamentoId);

        // 2) Monta PagamentosVM já carregado (sem abrir a janela)
        Task<PagamentosViewModel> CriarPagamentosViewModelAsync(int agendamentoId);

        // 3) Historico do cliente (dados prontos)
        Task<IReadOnlyList<AgendamentoHistoricoVM>> ObterHistoricoClienteAsync(int clienteId);
    }
}
