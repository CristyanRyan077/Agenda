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
            destino.ValorPago = origem.ValorPago;
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

            var q = _db.Agendamentos.AsNoTracking()
                .Where(a => a.Data >= inicio && a.Data <= fimInclusivo);

            if (servicoId.HasValue)
                q = q.Where(a => a.ServicoId == servicoId.Value);

            if (status.HasValue)
                q = q.Where(a => a.Status == status.Value);

            // sem Include: projeta apenas o necessário
            return q.Select(a => new FinanceiroRow
            {
                Id = a.Id,
                Data = a.Data,
                ServicoId = a.ServicoId,
                ServicoNome = a.Servico != null ? a.Servico.Nome : "—",
                ClienteNome = a.Cliente != null ? a.Cliente.Nome : null,
                Valor = a.Valor,
                ValorPago = a.ValorPago,
                Status = a.Status
            });
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

                var receita = await valid.SumAsync(a => a.Valor);
                System.Diagnostics.Debug.WriteLine($"[{T()}] [SRV {_sid}] depois receita ctx={_db.CtxId}");

                var recebido = await valid.SumAsync(a => a.ValorPago < a.Valor ? a.ValorPago : a.Valor);
                System.Diagnostics.Debug.WriteLine($"[{T()}] [SRV {_sid}] depois recebido ctx={_db.CtxId}");

                var aberto = await valid.SumAsync(a => a.Valor - (a.ValorPago < a.Valor ? a.ValorPago : a.Valor));
                var qtd = await valid.CountAsync();

                var concluidos = valid.Where(a => a.Status == StatusAgendamento.Concluido);
                var temConc = await concluidos.AnyAsync();
                var ticket = temConc ? Math.Round(await concluidos.AverageAsync(a => a.ValorPago < a.Valor ? a.ValorPago : a.Valor), 2) : 0;

                return new FinanceiroResumo { ReceitaBruta = receita, Recebido = recebido, EmAberto = Math.Max(0, aberto), QtdAgendamentos = qtd, TicketMedio = ticket };
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
            var baseQ = QueryFinanceiro(inicio, fim, servicoId, status);
            var valid = baseQ.Where(a => a.Status != StatusAgendamento.Cancelado);

            return valid.Where(a => a.Valor > a.ValorPago)
                .OrderBy(a => a.Data)
                .Select(a => new RecebivelDTO
                {
                    Id = a.Id,
                    Data = a.Data,
                    Cliente = a.ClienteNome,
                    Servico = a.ServicoNome,
                    Valor = a.Valor,
                    ValorPago = a.ValorPago,
                    Status = a.Status.ToString()
                })
                .ToListAsync();
        }

        public Task<List<ServicoResumoDTO>> ResumoPorServicoAsync(DateTime inicio, DateTime fim, int? servicoId = null, StatusAgendamento? status = null)
        {
            var baseQ = QueryFinanceiro(inicio, fim, servicoId, status);
            var valid = baseQ.Where(a => a.Status != StatusAgendamento.Cancelado);

            return valid
                .GroupBy(a => new { a.ServicoId, a.ServicoNome })
                .Select(g => new ServicoResumoDTO
                {
                    Servico = g.Key.ServicoNome,
                    Receita = g.Sum(x => x.ValorPago < x.Valor ? x.ValorPago : x.Valor),
                    Qtd = g.Count(),
                    TicketMedio = g.Average(x => x.ValorPago < x.Valor ? x.ValorPago : x.Valor)
                })
                .OrderByDescending(x => x.Receita)
                .ToListAsync();
        }
    }

}
