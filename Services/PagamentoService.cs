using AgendaNovo.Interfaces;
using AgendaNovo.Models;
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
        public async Task<List<HistoricoFinanceiroDto>> ListarHistoricoAsync(int agendamentoId)
        {
            using var db = _dbFactory.CreateDbContext();


            // 1. Pagamentos
            var pagamentos = await db.Pagamentos
                 .AsNoTracking()
                 .Where(p => p.AgendamentoId == agendamentoId)
                 .ToListAsync();

            // 2. Produtos
            var produtos = await db.AgendamentoProdutos
                .AsNoTracking()
                .Where(ap => ap.AgendamentoId == agendamentoId)
                .Include(ap => ap.Produto)
                .Include(ap => ap.Agendamento) // traz a data
                .ToListAsync();
            var historicoPagamentos = pagamentos.Select(p => new HistoricoFinanceiroDto(
                p.Id,
                p.DataPagamento,
                "Pagamento",
                p.Observacao ?? "Pagamento de serviço",
                p.Valor,
                p.Metodo
            ));
            var historicoProdutos = produtos
            .Where(ap => ap.Agendamento != null)
            .Select(ap => new HistoricoFinanceiroDto(
                ap.Id,
                ap.CreatedAt,
                "Produto",
                ap.Produto.Nome,
                ap.ValorUnitario * ap.Quantidade,
                null
            ));

            // 3. Junta tudo
            return historicoPagamentos
                .Union(historicoProdutos)
                .OrderBy(h => h.Data)
                .ToList();
        }
        public async Task AdicionarProdutoAoAgendamentoAsync(int agendamentoId,
            CriarProdutoAgendamentoDto dto,
            MetodoPagamento? metodo = null,
            string? observacao = null,
            DateTime? dataPagamento = null)
        {
            using var db = _dbFactory.CreateDbContext();
            await using var tx = await db.Database.BeginTransactionAsync();

            var entidade = new AgendamentoProduto
            {
                AgendamentoId = agendamentoId,
                ProdutoId = dto.ProdutoId,
                Quantidade = dto.Quantidade,
                ValorUnitario = dto.ValorUnitario,
                CreatedAt = DateTime.Now
            };

            db.AgendamentoProdutos.Add(entidade);
            await db.SaveChangesAsync();

            // pega o nome do produto para a observação
            var produto = await db.Produtos.AsNoTracking()
                                .Where(p => p.Id == dto.ProdutoId)
                                .Select(p => p.Nome)
                                .FirstAsync();

            db.Pagamentos.Add(new Pagamento
            {
                AgendamentoId = agendamentoId,
                Valor = entidade.ValorTotal,
                DataPagamento = dataPagamento ?? DateTime.Now,
                Metodo = metodo ?? MetodoPagamento.Pix,
                Observacao = observacao ?? $"Produto: {produto}",
                AgendamentoProdutoId = entidade.Id
            });
            await db.SaveChangesAsync();

            await tx.CommitAsync();
        }
        public async Task RemoverProdutoDoAgendamentoAsync(int agendamentoProdutoId)
        {
            using var db = _dbFactory.CreateDbContext();
            db.AgendamentoProdutos.Remove(new AgendamentoProduto { Id = agendamentoProdutoId });
            await db.SaveChangesAsync();
        }
    }
}
