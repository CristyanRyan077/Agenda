using AgendaNovo.Interfaces;
using AgendaNovo.Models;
using AgendaNovo.ViewModels;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static AgendaNovo.Agendamento;

namespace AgendaNovo.Services
{
    public class AgendamentoService : IAgendamentoService
    {
        private readonly AgendaContext _db;
        private readonly string _sid = Guid.NewGuid().ToString("N")[..6];
        private readonly IDbContextFactory<AgendaContext> _dbFactory;
        private static string T() => DateTime.Now.ToString("HH:mm:ss.fff");
        public AgendamentoService(AgendaContext db, IDbContextFactory<AgendaContext> dbFactory)
        {
            _db = db;
            System.Diagnostics.Debug.WriteLine($"[SRV NEW] {_sid} ctx={_db.CtxId}");
            _dbFactory = dbFactory;
        }
        public void AtivarSePendente(int agendamentoid)
        {
            var agendamento = GetById(agendamentoid);
            if (agendamento != null && (agendamento.Status == StatusAgendamento.Pendente))
            {
                agendamento.Status = StatusAgendamento.Concluido;
                Update(agendamento);
            }
        }
        public void ValorIncompleto(int agendamentoid)
        {
            var agendamento = GetById(agendamentoid);
            if (agendamento != null &&  agendamento.Status == StatusAgendamento.Concluido)
            {
                agendamento.Status = StatusAgendamento.Pendente;
                Update(agendamento);
            }
        }

        public List<Agendamento> GetAll()
        {
            return _db.Agendamentos
                .Include(a => a.Cliente)
                .Include(a => a.Crianca)
                .Include (a => a.Pacote)
                .Include (a => a.Servico)
                .Include (a => a.Pagamentos)
                .Include(a => a.AgendamentoProdutos)
                .AsNoTracking()
                .ToList();
        }



        public Agendamento? GetById(int id)
        {
            return _db.Agendamentos
                .Include(a => a.Cliente)
                .Include(a => a.Crianca)
                .Include(a => a.Pacote)
                .Include(a => a.Servico)
                .Include(a => a.Pagamentos)
                .Include(a => a.AgendamentoProdutos)
                .FirstOrDefault(a => a.Id == id);
        }
        public Agendamento? GetByIdAsNoTracking(int id)
        {
            return _db.Agendamentos
                .Include(a => a.Cliente)
                .Include(a => a.Crianca)
                .Include(a => a.Pacote)
                .Include(a => a.Servico)
                .Include(a => a.Pagamentos)
                .Include(a => a.AgendamentoProdutos)
                .AsNoTracking()
                .FirstOrDefault(a => a.Id == id);
        }
        private static int? ConverterIdadeParaMeses(int? idade, IdadeUnidade unidade)
        {
            if (!idade.HasValue) return null;
            return unidade switch
            {
                IdadeUnidade.Ano or IdadeUnidade.Anos => Math.Max(0, idade.Value * 12),
                IdadeUnidade.Mês or IdadeUnidade.Meses => Math.Max(0, idade.Value),
                _ => null
            };
        }

        public Agendamento Add(Agendamento agendamento)
        {
            Crianca? crianca = agendamento.Crianca;
            if (crianca == null && agendamento.CriancaId.HasValue)
                crianca = _db.Criancas.Find(agendamento.CriancaId.Value);

            agendamento.Mesversario = ConverterIdadeParaMeses(crianca?.Idade, crianca?.IdadeUnidade ?? IdadeUnidade.Meses);

            _db.Agendamentos.Add(agendamento);
            _db.SaveChanges();
            return agendamento;
        }
        private void CopiarDados(Agendamento origem, Agendamento destino)
        {
            destino.Data = origem.Data;
            destino.Horario = origem.Horario;
            destino.Tema = origem.Tema;
            destino.Valor = origem.Valor;
            destino.Pagamentos = origem.Pagamentos;
            destino.PacoteId = origem.PacoteId;
            destino.ServicoId = origem.ServicoId;
            destino.ClienteId = origem.ClienteId;
            destino.CriancaId = origem.CriancaId;
        }

        public void Update(Agendamento agendamento)
        {
            var existente = _db.Agendamentos
            .Include(a => a.Cliente)
            .Include(a => a.Crianca)
            .Include(a => a.Pacote)
            .Include(a => a.Servico)
            .Include(a => a.Pagamentos)
            .Include(a => a.AgendamentoProdutos)
            .FirstOrDefault(a => a.Id == agendamento.Id);
            if (existente == null)
            {
                MessageBox.Show("agendamento inexistente");
                return;
            }

            // Detectar alterações relevantes
            bool criancaAlterada = existente.CriancaId != agendamento.CriancaId;
            bool dataAlterada = existente.Data.Date != agendamento.Data.Date;
            bool servicoAlterado = existente.ServicoId != agendamento.ServicoId;
            CopiarDados(agendamento, existente);
            Crianca? crianca = existente.Crianca;

            if (crianca == null && existente.CriancaId.HasValue)
                crianca = _db.Criancas.Find(existente.CriancaId.Value);

            if (criancaAlterada || dataAlterada || servicoAlterado)
                existente.Mesversario = ConverterIdadeParaMeses(crianca?.Idade, crianca?.IdadeUnidade ?? IdadeUnidade.Meses);

        }

        public void Delete(int id)
        {
            var ag = _db.Agendamentos.Find(id);
            if (ag == null) return;
            _db.Agendamentos.Remove(ag);
            _db.SaveChanges();
        }

        public List<Agendamento> GetByDate(DateTime data)
        {
            return _db.Agendamentos
                .Include(a => a.Cliente)
                .Include(a => a.Crianca)
                .Include(a => a.Pacote)
                .Include(a => a.Servico)
                .Include(a => a.Pagamentos)
                .Include(a => a.AgendamentoProdutos)
                .Where(a => a.Data.Date == data.Date)
                .AsNoTracking()
                .ToList();
        }



        public List<Agendamento> GetByCliente(int clienteId)
        {
            return _db.Agendamentos
                .Include(a => a.Cliente)
                .Include(a => a.Crianca)
                .Include(a => a.Pacote)
                .Include(a => a.Servico)
                .Include(a => a.Pagamentos)
                .Include(a => a.AgendamentoProdutos)
                .Where(a => a.ClienteId == clienteId)
                .AsNoTracking()
                .ToList();
        }

        public List<Agendamento> GetByCrianca(int criancaId)
        {
            return _db.Agendamentos
                .Include(a => a.Cliente)
                .Include(a => a.Crianca)
                .Include(a => a.Pacote)
                .Include(a => a.Servico)
                .Include(a => a.Pagamentos)
                .Include(a => a.AgendamentoProdutos)
                .Where(a => a.CriancaId == criancaId)
                .AsNoTracking()
                .ToList();
        }
        public void UpdateEtapas(Agendamento agendamento)
        {
            using var db = _dbFactory.CreateDbContext();

            var existente = db.Agendamentos.FirstOrDefault(a => a.Id == agendamento.Id);
            if (existente == null) return;

            // Atualiza apenas os campos de etapas
            existente.EscolhaFeitaEm = agendamento.EscolhaFeitaEm;
            existente.TratadasEm = agendamento.TratadasEm;
            existente.EntregueEm = agendamento.EntregueEm;

            var e = db.Entry(existente);
            e.Property(a => a.EscolhaFeitaEm).IsModified = true;
            e.Property(a => a.TratadasEm).IsModified = true;
            e.Property(a => a.ProducaoConcluidaEm).IsModified = true;
            e.Property(a => a.EntregueEm).IsModified = true;

            db.SaveChanges();
        }
        public void UpdateItens(int agendamentoId, List<AgendamentoProduto> itens)
        {
            using var db = _dbFactory.CreateDbContext();

            var existentes = db.AgendamentoProdutos
                .Where(p => p.AgendamentoId == agendamentoId)
                .ToList();

            foreach (var it in itens)
            {
                var existente = existentes.FirstOrDefault(x => x.Id == it.Id);
                if (existente == null) continue;

                existente.EnviadoParaProducaoEm = it.EnviadoParaProducaoEm;
                existente.ProducaoConcluidaEm = it.ProducaoConcluidaEm;

                db.Entry(existente).Property(x => x.EnviadoParaProducaoEm).IsModified = true;
                db.Entry(existente).Property(x => x.ProducaoConcluidaEm).IsModified = true;
            }

            db.SaveChanges();
        }

        public async Task AtualizarFotosAsync(int agendamentoId, FotosReveladas fotos)
        {
            using var db = _dbFactory.CreateDbContext();
            var agendamento = await db.Agendamentos.FindAsync(agendamentoId);
            if (agendamento is null) return;

            agendamento.Fotos = fotos;
            await db.SaveChangesAsync();
        }
        // ========= FINANCEIRO =========

        // Query base (projeção leve, 100% traduzível pelo EF)
        public IQueryable<FinanceiroRow> QueryFinanceiro(
            DateTime inicio, DateTime fim,
            int? servicoId = null,
            StatusAgendamento? status = null)
        {
            var fimInclusivo = fim.Date.AddDays(1).AddTicks(-1);

            var baseQ =
                from a in _db.Agendamentos.AsNoTracking()
                where a.Data >= inicio && a.Data <= fimInclusivo
                select a;
            if (servicoId.HasValue)
                baseQ = baseQ.Where(a => a.ServicoId == servicoId.Value);
            if (status.HasValue)
                baseQ = baseQ.Where(a => a.Status == status.Value);

            // sem Include: projeta apenas o necessário
            var q =
             from a in baseQ
             join s in _db.Servicos.AsNoTracking() on a.ServicoId equals s.Id into sj
             from s in sj.DefaultIfEmpty()
             join c in _db.Clientes.AsNoTracking() on a.ClienteId equals c.Id into cj
             from c in cj.DefaultIfEmpty()
             select new FinanceiroRow
             {
                 Id = a.Id,
                 Data = a.Data,
                 ServicoId = a.ServicoId,
                 ServicoNome = s != null ? s.Nome : "—",
                 ClienteNome = c != null ? c.Nome : null,
                 Valor = a.Valor,
                 ValorPago = a.ValorPago,
                 Status = a.Status
             };

            return q;
        }

        public async Task<FinanceiroResumo> CalcularKpisAsync(DateTime inicio, DateTime fim, int? servicoId = null, int? produtoId = null, StatusAgendamento? status = null)
        {
            var tid = Environment.CurrentManagedThreadId;
            System.Diagnostics.Debug.WriteLine($"[{T()}] [SRV {_sid}] CalcularKpis START ctx={_db.CtxId} tid={tid}");
            try
            {
                var baseQ = _db.Agendamentos.AsNoTracking()
                    .Where(a => a.Data >= inicio && a.Data <= fim);
                if (servicoId.HasValue) baseQ = baseQ.Where(a => a.ServicoId == servicoId.Value);
                if (status.HasValue) baseQ = baseQ.Where(a => a.Status == status.Value);

                var valid = baseQ.Where(a => a.Status != StatusAgendamento.Cancelado);
                var q = valid.Select(a => new
                {
                    a.Valor,
                    Pago = a.Pagamentos.Sum(p => (decimal?)p.Valor) ?? 0m,
                    a.Status
                });
                var kpi = await q
                 .GroupBy(a => 1)
                 .Select(g => new
                 {
                     Receita = g.Sum(a => a.Valor),
                     Recebido = g.Sum(a => a.Pago < a.Valor ? a.Pago : a.Valor),
                     Aberto = g.Sum(a => a.Valor - (a.Pago < a.Valor ? a.Pago : a.Valor)),
                     Qtd = g.Count(),
                     TicketMedio = g.Where(a => a.Status == StatusAgendamento.Concluido)
                       .Average(a => (decimal?)(a.Pago < a.Valor ? a.Pago : a.Valor)),

                 })
                 .FirstOrDefaultAsync();


                 var pagamentosPorItemQ =
                    from p in _db.Set<Agendamento.Pagamento>().AsNoTracking()
                    where p.AgendamentoProdutoId != null
                    group p by p.AgendamentoProdutoId!.Value into g
                    select new { AgendamentoProdutoId = g.Key, Pago = g.Sum(x => x.Valor) };

                // 2) Linhas de venda (join AgendamentoProdutos x agendamentos válidos x pagamentos por item)
                var itensQ =
                    from ap in _db.AgendamentoProdutos.AsNoTracking()
                    join a in valid on ap.AgendamentoId equals a.Id
                    join pg in pagamentosPorItemQ on ap.Id equals pg.AgendamentoProdutoId into pgj
                    from pg in pgj.DefaultIfEmpty()
                    where !produtoId.HasValue || ap.ProdutoId == produtoId.Value
                    select new
                    {
                        Quantidade = ap.Quantidade,
                        ValorTotal = ap.Quantidade * ap.ValorUnitario, // usar expressão (propriedade calculada não traduz)
                        Pago = pg == null ? 0m : pg.Pago
                    };

                // 3) Materializa as linhas e agrega em memória (estável no EF Core)
                var itens = await itensQ.ToListAsync();

                var receitaProdutos = itens.Sum(i => Math.Min(i.Pago, i.ValorTotal));
                var qtdProdutos = itens.Sum(i => i.Quantidade);
                var ticketMedioProd = qtdProdutos > 0 ? receitaProdutos / qtdProdutos : 0m;

                // ==============================
                // Retorno unificado
                // ==============================
                return new FinanceiroResumo
                {
                    ReceitaBruta = kpi?.Receita ?? 0m,
                    Recebido = kpi?.Recebido ?? 0m,
                    EmAberto = Math.Max(0, kpi?.Aberto ?? 0m),
                    QtdAgendamentos = kpi?.Qtd ?? 0,
                    TicketMedio = Math.Round(kpi?.TicketMedio ?? 0m, 2),

                    // Produtos
                    ReceitaProdutos = receitaProdutos,
                    QtdProdutos = qtdProdutos,
                    TicketMedioProdutos = Math.Round(ticketMedioProd, 2)
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[{T()}] [SRV {_sid}] CalcularKpis EX: {ex.GetType().Name} - {ex.Message}");
                throw;
            }
            finally
            {
                System.Diagnostics.Debug.WriteLine($"[{T()}] [SRV {_sid}] CalcularKpis END ctx={_db.CtxId}");
            }
        }

        public Task<List<RecebivelDTO>> ListarEmAbertoAsync(DateTime inicio, DateTime fim, int? servicoId = null, StatusAgendamento? status = null)
        {
            var fimIncl = fim.Date.AddDays(1).AddTicks(-1);

            var baseQ = _db.Agendamentos.AsNoTracking()
                .Where(a => a.Data >= inicio && a.Data <= fimIncl);

            if (servicoId.HasValue) baseQ = baseQ.Where(a => a.ServicoId == servicoId.Value);
            if (status.HasValue) baseQ = baseQ.Where(a => a.Status == status.Value);

            var valid = baseQ.Where(a => a.Status != StatusAgendamento.Cancelado);

            var q = from a in valid
                    join s in _db.Servicos.AsNoTracking() on a.ServicoId equals s.Id into sj
                    from s in sj.DefaultIfEmpty()
                    join c in _db.Clientes.AsNoTracking() on a.ClienteId equals c.Id into cj
                    from c in cj.DefaultIfEmpty()
                    let pago = a.Pagamentos.Sum(p => (decimal?)p.Valor) ?? 0m
                    where a.Valor > pago
                    orderby a.Data
                    select new RecebivelDTO
                    {
                        Id = a.Id,
                        Data = a.Data,
                        Cliente = c != null ? c.Nome : null,
                        Servico = s != null ? s.Nome : "—",
                        Valor = a.Valor,
                        ValorPago = pago,
                        Status = a.Status.ToString()
                    };

            return q.ToListAsync();
        }

        public Task<List<ServicoResumoDTO>> ResumoPorServicoAsync(
         DateTime inicio, DateTime fim, int? servicoId = null, StatusAgendamento? status = null)
        {
            var fimIncl = fim.Date.AddDays(1).AddTicks(-1);

            var baseQ = _db.Agendamentos.AsNoTracking()
                .Where(a => a.Data >= inicio && a.Data <= fimIncl);

            if (servicoId.HasValue) baseQ = baseQ.Where(a => a.ServicoId == servicoId.Value);
            if (status.HasValue) baseQ = baseQ.Where(a => a.Status == status.Value);

            var valid = baseQ.Where(a => a.Status != StatusAgendamento.Cancelado);

            var query =
                from g in
                    (from a in valid
                     let pago = a.Pagamentos.Sum(p => (decimal?)p.Valor) ?? 0m
                     group new { a, pago } by a.ServicoId into grp
                     select new
                     {
                         ServicoId = grp.Key,
                         Receita = grp.Sum(x => x.pago < x.a.Valor ? x.pago : x.a.Valor),
                         Qtd = grp.Count(),
                         TicketMedio = grp.Average(x => x.pago < x.a.Valor ? x.pago : x.a.Valor)
                     })
                join s in _db.Servicos.AsNoTracking() on g.ServicoId equals s.Id into sj
                from s in sj.DefaultIfEmpty()
                orderby g.Receita descending
                select new ServicoResumoDTO
                {
                    Servico = s != null ? s.Nome : "—",
                    Receita = g.Receita,
                    Qtd = g.Qtd,
                    TicketMedio = g.TicketMedio
                };

            return query.ToListAsync();
        }
        public async Task<List<ProdutoResumoVM>> ResumoPorProdutoAsync(
     DateTime inicio, DateTime fim, int? produtoId = null, StatusAgendamento? status = null)
        {
            var fimIncl = fim.Date.AddDays(1).AddTicks(-1);

            // Agendamentos válidos no intervalo (exceto cancelados e com filtro de status, se houver)
            var agdsValid = _db.Agendamentos.AsNoTracking()
                .Where(a => a.Data >= inicio && a.Data <= fimIncl)
                .Where(a => a.Status != StatusAgendamento.Cancelado);

            if (status.HasValue)
                agdsValid = agdsValid.Where(a => a.Status == status.Value);

            // Pagamentos por item (AgendamentoProdutoId) agregados no servidor
            var pagamentosPorItemQ =
                from p in _db.Pagamentos.AsNoTracking() // ou _db.Set<Agendamento.Pagamento>()
                where p.AgendamentoProdutoId != null
                group p by p.AgendamentoProdutoId!.Value into g
                select new { AgendamentoProdutoId = g.Key, Pago = g.Sum(x => x.Valor) };

            // “Linhas” (itens vendidos) já com valores necessários – ainda tudo no servidor
            var itensQ =
                from ap in _db.AgendamentoProdutos.AsNoTracking()
                join a in agdsValid on ap.AgendamentoId equals a.Id
                join pg in pagamentosPorItemQ on ap.Id equals pg.AgendamentoProdutoId into pgj
                from pg in pgj.DefaultIfEmpty()
                where !produtoId.HasValue || ap.ProdutoId == produtoId.Value
                select new
                {
                    ap.ProdutoId,
                    Quantidade = ap.Quantidade,
                    ValorTotal = ap.Quantidade * ap.ValorUnitario,        // evita propriedade não mapeada
                    Pago = pg == null ? 0m : pg.Pago
                };

            // 🔻 materializa apenas as “linhas” (tamanho = nº de AgendamentoProduto no período/filtrado)
            var itens = await itensQ.ToListAsync();

            // Busca nomes dos produtos só para os IDs presentes
            var ids = itens.Select(i => i.ProdutoId).Distinct().ToList();
            var nomePorId = await _db.Produtos.AsNoTracking()
                .Where(p => ids.Contains(p.Id))
                .Select(p => new { p.Id, p.Nome })
                .ToDictionaryAsync(x => x.Id, x => x.Nome);

            // Agrega em memória (super simples, sem esquisitices de tradução)
            var agregados = itens
                .GroupBy(i => i.ProdutoId)
                .Select(g =>
                {
                    var receita = g.Sum(i => Math.Min(i.Pago, i.ValorTotal));
                    var qtd = g.Sum(i => i.Quantidade);
                    return new ProdutoResumoVM
                    {
                        Produto = nomePorId.TryGetValue(g.Key, out var nome) ? nome : "—",
                        Receita = receita,
                        Qtd = qtd,
                        TicketMedio = qtd > 0 ? receita / qtd : 0m
                    };
                })
                .OrderByDescending(x => x.Receita)
                .ToList();

            return agregados;
        }
    }

}
