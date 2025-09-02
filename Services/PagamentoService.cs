using AgendaNovo.Interfaces;
using AgendaNovo.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AgendaNovo.Agendamento;

namespace AgendaNovo.Services
{
    public class PagamentoService : IPagamentoService
    {

        private readonly AgendaContext _db;
        public PagamentoService(AgendaContext db) => _db = db;

        public async Task<ResumoAgendamentoDto> ObterResumoAgendamentoAsync(int agendamentoId)
        {
            var q =
                from a in _db.Agendamentos.AsNoTracking()
                join c in _db.Clientes.AsNoTracking() on a.ClienteId equals c.Id
                join s in _db.Servicos.AsNoTracking() on a.ServicoId equals s.Id into sj
                from s in sj.DefaultIfEmpty()
                where a.Id == agendamentoId
                select new ResumoAgendamentoDto(c.Nome, s != null ? s.Nome : "—", a.Data, a.Valor);

            return await q.FirstAsync();
        }

        public async Task<List<PagamentoDto>> ListarPagamentosAsync(int agendamentoId)
        {
            return await _db.Pagamentos.AsNoTracking()
                .Where(p => p.AgendamentoId == agendamentoId)
                .OrderBy(p => p.DataPagamento)
                .Select(p => new PagamentoDto(p.Id, p.DataPagamento, p.Valor, p.Metodo, p.Observacao))
                .ToListAsync();
        }

        public async Task AdicionarPagamentoAsync(int agendamentoId, CriarPagamentoDto dto)
        {
            _db.Pagamentos.Add(new Pagamento
            {
                AgendamentoId = agendamentoId,
                Valor = dto.Valor,
                DataPagamento = dto.DataPagamento == default ? DateTime.Now : dto.DataPagamento,
                Metodo = dto.Metodo,
                Observacao = dto.Observacao
            });
            await _db.SaveChangesAsync();
        }

        public async Task AtualizarPagamentoAsync(AtualizarPagamentoDto dto)
        {
            var p = new Pagamento { Id = dto.Id }; // no-track update
            _db.Attach(p);
            p.Valor = dto.Valor;
            p.DataPagamento = dto.DataPagamento;
            p.Metodo = dto.Metodo;
            p.Observacao = dto.Observacao;
            await _db.SaveChangesAsync();
        }

        public async Task RemoverPagamentoAsync(int pagamentoId)
        {
            _db.Pagamentos.Remove(new Pagamento { Id = pagamentoId });
            await _db.SaveChangesAsync();
        }
    }
}
