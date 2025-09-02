using AgendaNovo.Interfaces;
using AgendaNovo.Models;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AgendaNovo.Services
{
    public class AgendamentoService : IAgendamentoService
    {
        private readonly AgendaContext _db;
        private readonly string _sid = Guid.NewGuid().ToString("N")[..6];
        private static string T() => DateTime.Now.ToString("HH:mm:ss.fff");
        public AgendamentoService(AgendaContext db)
        {
            _db = db;
            System.Diagnostics.Debug.WriteLine($"[SRV NEW] {_sid} ctx={_db.CtxId}");
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
                .FirstOrDefault(a => a.Id == id);
        }

        public Agendamento Add(Agendamento agendamento)
        {
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
            .FirstOrDefault(a => a.Id == agendamento.Id);
            if (existente != null)
            {
                CopiarDados(agendamento, existente);
                _db.SaveChanges();
            }
            else
                MessageBox.Show("agendamento inexistente");
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
                .Where(a => a.CriancaId == criancaId)
                .AsNoTracking()
                .ToList();
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

        public async Task<FinanceiroResumo> CalcularKpisAsync(DateTime inicio, DateTime fim, int? servicoId = null, StatusAgendamento? status = null)
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
                       .Average(a => (decimal?)(a.Pago < a.Valor ? a.Pago : a.Valor))
                 })
                 .FirstOrDefaultAsync();


                return new FinanceiroResumo
                {
                    ReceitaBruta = kpi?.Receita ?? 0m,
                    Recebido = kpi?.Recebido ?? 0m,
                    EmAberto = Math.Max(0, kpi?.Aberto ?? 0m),
                    QtdAgendamentos = kpi?.Qtd ?? 0,
                    TicketMedio = Math.Round(kpi?.TicketMedio ?? 0m, 2)
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
    }

}
