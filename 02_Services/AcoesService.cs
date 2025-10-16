using AgendaNovo._01_Interfaces;
using AgendaNovo.Dtos;
using AgendaNovo.Interfaces;
using AgendaNovo.Models;
using AgendaNovo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgendaNovo._02_Services
{
    public class AcoesService : IAcoesService
    {
        private readonly IAgendamentoService _ag;
        private readonly IClienteService _cli;
        private readonly IPacoteService _pac;
        private readonly IServicoService _srv;
        private readonly IPagamentoService _pag;
        private readonly IProdutoService _prod;

        public AcoesService(
            IAgendamentoService ag, IClienteService cli,
            IPacoteService pac, IServicoService srv,
            IPagamentoService pag, IProdutoService prod)
        {
            _ag = ag; _cli = cli; _pac = pac; _srv = srv; _pag = pag; _prod = prod;
        }

        public async Task<EditarAgendamentoDto?> PrepararEdicaoAsync(int id)
        {
            // nada de mexer em VM aqui!
            var a = _ag.GetByIdAsNoTracking(id);
            if (a == null) return null;

            // carrega listas auxiliares que a tela usa
            var servicos = _srv.GetAll().ToList();
            var pacotes = _pac.GetAll().ToList();
            var pacotesFiltrados = a.ServicoId.HasValue
                ? pacotes.Where(p => p.ServicoId == a.ServicoId.Value).ToList()
                : pacotes;

            return new EditarAgendamentoDto
            {
                Agendamento = a,
                Cliente = a.Cliente,
                Servicos = servicos,
                Pacotes = pacotes,
                PacotesFiltrados = pacotesFiltrados
            };
        }

        public async Task<PagamentosViewModel> CriarPagamentosViewModelAsync(int agendamentoId)
        {
            var vm = new PagamentosViewModel(_pag, agendamentoId, _ag, _cli, _prod);
            await vm.CarregarAsync();
            return vm;
        }

        public async Task<IReadOnlyList<AgendamentoHistoricoVM>> ObterHistoricoClienteAsync(int clienteId)
        {
            var lista = _cli.GetAgendamentos(clienteId) ?? new List<Agendamento>();
            var historico = lista
                .OrderByDescending(a => a.Data)
                .Select(a => new AgendamentoHistoricoVM { Agendamento = a, NumeroMes = a.Mesversario })
                .ToList();
            return historico;
        }
    }


}
