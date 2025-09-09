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

        private readonly IDbContextFactory<AgendaContext> _dbFactory;
        public PagamentoService(IDbContextFactory<AgendaContext> dbFactory)
        => _dbFactory = dbFactory;

        public async Task<ResumoAgendamentoDto> ObterResumoAgendamentoAsync(int agendamentoId)
        {
            using var db = _dbFactory.CreateDbContext();
            var q =
                from a in db.Agendamentos.AsNoTracking()
                join c in db.Clientes.AsNoTracking() on a.ClienteId equals c.Id
                join s in db.Servicos.AsNoTracking() on a.ServicoId equals s.Id into sj
                from s in sj.DefaultIfEmpty()
                where a.Id == agendamentoId
                select new ResumoAgendamentoDto(c.Id, c.Nome, a.Fotos, s != null ? s.Nome : "—", a.Data, a.Valor);

            return await q.FirstAsync();
        }

        public async Task<List<PagamentoDto>> ListarPagamentosAsync(int agendamentoId)
        {
            using var db = _dbFactory.CreateDbContext();

            return await db.Pagamentos.AsNoTracking()
                .Where(p => p.AgendamentoId == agendamentoId)
                .OrderBy(p => p.DataPagamento)
                .Select(p => new PagamentoDto(p.Id, p.DataPagamento, p.Valor, p.Metodo, p.Observacao))
                .ToListAsync();
        }

        public async Task AdicionarPagamentoAsync(int agendamentoId, CriarPagamentoDto dto)
        {
            using var db = _dbFactory.CreateDbContext();
            db.Pagamentos.Add(new Pagamento
            {
                AgendamentoId = agendamentoId,
                Valor = dto.Valor,
                DataPagamento = dto.DataPagamento == default ? DateTime.Now : dto.DataPagamento,
                Metodo = dto.Metodo,
                Observacao = dto.Observacao
            });
            await db.SaveChangesAsync();
        }

        public async Task AtualizarPagamentoAsync(AtualizarPagamentoDto dto)
        {
            using var db = _dbFactory.CreateDbContext();
            var p = new Pagamento { Id = dto.Id }; // no-track update
            db.Attach(p);
            p.Valor = dto.Valor;
            p.DataPagamento = dto.DataPagamento;
            p.Metodo = dto.Metodo;
            p.Observacao = dto.Observacao;
            await db.SaveChangesAsync();
        }

        public async Task RemoverPagamentoAsync(int pagamentoId)
        {
            using var db = _dbFactory.CreateDbContext();
            db.Pagamentos.Remove(new Pagamento { Id = pagamentoId });
            await db.SaveChangesAsync();
        }
    }
}
