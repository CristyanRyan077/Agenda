using AgendaNovo.Controles;
using AgendaNovo.Converters;
using AgendaNovo.Helpers;
using AgendaNovo.Interfaces;
using AgendaNovo.Migrations;
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
using static AgendaNovo.Services.AgendamentoService;

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
            if (agendamento != null && (agendamento.Status != StatusAgendamento.Concluido))
            {
                agendamento.Status = StatusAgendamento.Concluido;
                Update(agendamento);
            }
        }
        public void ValorIncompleto(int agendamentoid)
        {
            var agendamento = GetById(agendamentoid);
            if (agendamento != null &&  agendamento.Status != StatusAgendamento.Concluido)
            {
                agendamento.Status = StatusAgendamento.Pendente;
                Update(agendamento);
            }
        }
        public void UpdateStatus(int id, StatusAgendamento novoStatus)
        {
            using var ctx = _dbFactory.CreateDbContext(); // NOVO contexto
            var rows = ctx.Agendamentos
                          .Where(a => a.Id == id)
                          .ExecuteUpdate(s => s.SetProperty(a => a.Status, novoStatus));
            System.Diagnostics.Debug.WriteLine($"[SRV] ExecuteUpdate rows={rows} id={id} status={novoStatus}");
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
                .Include(a => a.Etapas)
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
                .Include(a => a.Etapas)
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
                .Include(a => a.Etapas)
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
            var local = _db.Agendamentos.Local.FirstOrDefault(a => a.Id == agendamento.Id);
            if (local != null)
                _db.Entry(local).State = EntityState.Detached;
            agendamento.Id = 0;

            Crianca? crianca = null;
            if (agendamento.CriancaId.HasValue)
                crianca = _db.Criancas
                             .AsNoTracking()
                             .FirstOrDefault(c => c.Id == agendamento.CriancaId.Value);

            agendamento.Mesversario = ConverterIdadeParaMeses(
                crianca?.Idade, crianca?.IdadeUnidade ?? IdadeUnidade.Meses);

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
            .Include(a => a.Etapas)
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
                .Include(a => a.Etapas)
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
                .Include(a => a.Etapas)
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
                .Include(a => a.Etapas)
                .Where(a => a.CriancaId == criancaId)
                .AsNoTracking()
                .ToList();
        }

        public AgendamentoEtapa AddOrUpdateEtapa(int agendamentoId, EtapaFotos etapa, DateTime data, string? obs)
        {
            using var db = _dbFactory.CreateDbContext();

            var e = db.AgendamentoEtapas
                      .FirstOrDefault(x => x.AgendamentoId == agendamentoId && x.Etapa == etapa);

            if (e == null)
            {
                e = new AgendamentoEtapa
                {
                    AgendamentoId = agendamentoId,
                    Etapa = etapa,
                    DataConclusao = data,
                    Observacao = obs
                };
                db.AgendamentoEtapas.Add(e);
            }
            else
            {
                e.DataConclusao = data;
                e.Observacao = obs;
                e.UpdatedAt = DateTime.UtcNow;
            }

            db.SaveChanges();
            return e;
        }

        public List<AgendamentoAtrasoDTO> GetAgendamentosComFotosAtrasadas(DateTime hoje)
        {
            // puxa tudo que precisamos
            var itens = _db.Agendamentos
                .Include(a => a.Cliente)
                .Include(a => a.Crianca)
                .Include(a => a.Etapas)
                .Include(a => a.Pacote)
                .Include(a => a.Servico)
                .Include(a => a.Pagamentos)
                .Include(a => a.AgendamentoProdutos)
                .AsNoTracking()
                .ToList();

            var list = new List<AgendamentoAtrasoDTO>();

            foreach (var a in itens)
            {
                // prazos (derivados)
                int prazoTratar = (a.Servico?.PrazoTratarDias ?? 0) > 0 ? a.Servico!.PrazoTratarDias : 3;
                DateTime prevTrat = BusinessDays.AddBusinessDays(a.Data.Date, prazoTratar);
                DateTime prevRevelar = BusinessDays.AddBusinessDays(a.Data.Date, 15);
                DateTime prevEntrega = BusinessDays.AddBusinessDays(a.Data.Date, 30);

                bool Concluida(EtapaFotos etapa) =>
                 a.Etapas?.Any(e => e.Etapa == etapa && IsConcluded(e.DataConclusao)) == true;
                static bool IsConcluded(DateTime data) => data > DateTime.MinValue;

                

                bool conclTratamento = Concluida(EtapaFotos.Tratamento);
                bool conclRevelar = Concluida(EtapaFotos.Revelar);
                bool conclEntrega = Concluida(EtapaFotos.Entrega);
                // regra de progressão: se uma etapa posterior está concluída,
                // considere as anteriores como implicitamente concluídas
                if (conclEntrega)
                {
                    // tudo concluído, sem atraso
                    continue;
                }
                if (conclRevelar)
                {
                    // tratar/revelar concluídos; avalia só Entrega
                    conclTratamento = true;
                }
                else if (conclTratamento)
                {
                    // avalia Revelar e Entrega
                }
                else
                {
                    // nada concluído ainda -> avalia Tratamento, Revelar e Entrega
                }
                var atrasos = new List<(FotoAtrasoTipo Tipo, DateTime Previsto)>();


                if (!conclTratamento && hoje.Date > prevTrat.Date)
                    atrasos.Add((FotoAtrasoTipo.Tratamento, prevTrat));
                if (!conclRevelar && hoje.Date > prevRevelar.Date)
                    atrasos.Add((FotoAtrasoTipo.Revelar, prevRevelar));

                if (!conclEntrega && hoje.Date > prevEntrega.Date)
                    atrasos.Add((FotoAtrasoTipo.Entrega, prevEntrega));

                if (atrasos.Count > 0)
                    list.Add(new AgendamentoAtrasoDTO { Agendamento = a, Atrasos = atrasos });
            }

            return list;
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

        public bool AbrirEtapaDialog(SetEtapaParam p)
        {
            if (p?.Agendamento is null) return false;

            var vm = new EtapaDialogViewModel
            {
                AgendamentoId = p.Agendamento.Id,
                Etapa = p.Etapa,
                DataConclusao = DateTime.Today,
                Observacao = null
            };

            var dialog = new EtapaDialogView { DataContext = vm };
            var ok = dialog.ShowDialog() == true;
            if (!ok) return false;

            // Persistir
            var saved = AddOrUpdateEtapa(
                vm.AgendamentoId, vm.Etapa, vm.DataConclusao, vm.Observacao);

            // Atualizar a instância em memória do chamador
            var local = p.Agendamento.Etapas.FirstOrDefault(x => x.Etapa == saved.Etapa);
            if (local == null)
            {
                p.Agendamento.Etapas.Add(new AgendaNovo.Models.AgendamentoEtapa
                {
                    Id = saved.Id,
                    AgendamentoId = saved.AgendamentoId,
                    Etapa = saved.Etapa,
                    DataConclusao = saved.DataConclusao,
                    Observacao = saved.Observacao,
                    CreatedAt = saved.CreatedAt,
                    UpdatedAt = saved.UpdatedAt
                });
            }
            else
            {
                local.DataConclusao = saved.DataConclusao;
                local.Observacao = saved.Observacao;
                local.UpdatedAt = saved.UpdatedAt;
                var idx = p.Agendamento.Etapas.IndexOf(local);
                p.Agendamento.Etapas[idx] = local;
            }

            p.Agendamento.NotifyEtapasChanged();
            return true;
        }
        public async Task<List<FotoProcessoVM>> ListarProcessoFotosAsync(
    DateTime inicio, DateTime fim, EtapaStatus? status = null, string? clienteNome = null)
        {
            var q = _db.Agendamentos
                .AsNoTracking()
                .Include(a => a.Cliente)
                .Include(a => a.Crianca)
                .Include(a => a.Servico)
                .Include(a => a.Etapas)
                .Where(a => a.Data >= inicio && a.Data <= fim);

            if (!string.IsNullOrWhiteSpace(clienteNome))
            {
                var termo = clienteNome.Trim().ToLower();
                q = q.Where(a => a.Cliente.Nome.ToLower().Contains(termo));
            }

            var hoje = DateTime.Today;

            var lista = await q
                .OrderBy(a => a.Data)
                .Select(a => new
                {
                    a.Id,
                    a.Data,
                    ClienteNome = a.Cliente.Nome,
                    Crianca = a.Crianca.Nome,
                    Telefone = a.Cliente.Telefone,
                    Mesversario = a.Mesversario,
                    Servico = a.Servico.Nome,
                    PrazoTratarDias = (int?)a.Servico.PrazoTratarDias, 
                    Etapas = a.Etapas.Select(e => new { e.Etapa, e.DataConclusao })
                })
                .ToListAsync();

            static bool Ok(DateTime? dt) => dt.HasValue && dt.Value > DateTime.MinValue;

            var result = new List<FotoProcessoVM>();

            foreach (var a in lista)
            {
                // datas de conclusão por etapa (se existirem)
                DateTime? dtEscolha = a.Etapas.FirstOrDefault(e => e.Etapa == EtapaFotos.Escolha)?.DataConclusao;
                DateTime? dtTrat = a.Etapas.FirstOrDefault(e => e.Etapa == EtapaFotos.Tratamento)?.DataConclusao;
                DateTime? dtRevelar = a.Etapas.FirstOrDefault(e => e.Etapa == EtapaFotos.Revelar)?.DataConclusao;
                DateTime? dtEntrega = a.Etapas.FirstOrDefault(e => e.Etapa == EtapaFotos.Entrega)?.DataConclusao;

                // prazos por etapa (âncoras corretas)
                int prazoTratarDias = (a.PrazoTratarDias ?? 0) > 0 ? a.PrazoTratarDias!.Value : 3;
                int prazoRevelarDias = 15;
                int prazoEntregaDias = 30;

                // --- Ancoragem: sempre use a data de conclusão da etapa anterior quando houver ---
                // anchor para tratamento: se houve 'Escolha', conta a partir dela; senão a.Data
                DateTime anchorTrat = Ok(dtEscolha) ? dtEscolha!.Value.Date : a.Data.Date;
                // anchor para revelar: se houve 'Tratamento', conta a partir dele; senão usa anchorTrat (que já cai para escolha/a.Data)
                DateTime anchorRevelar = Ok(dtTrat) ? dtTrat!.Value.Date : anchorTrat;
                // anchor para entrega: se houve 'Revelar', conta a partir dele; senão usa anchorRevelar
                DateTime anchorEntrega = Ok(dtRevelar) ? dtRevelar!.Value.Date : anchorRevelar;

                DateTime prazoTrat = BusinessDays.AddBusinessDays(anchorTrat, prazoTratarDias).Date;
                DateTime prazoRevelar = anchorRevelar.AddDays(prazoRevelarDias).Date;
                DateTime prazoEntrega = anchorEntrega.AddDays(prazoEntregaDias).Date;

                EtapaFotos proxima;
                if (Ok(dtEntrega))
                {
                    proxima = EtapaFotos.Entrega; // “só para efeito de status/texto” (já concluído)
                }
                else if (Ok(dtRevelar))
                {
                    proxima = EtapaFotos.Entrega;
                }
                else if (Ok(dtTrat))
                {
                    proxima = EtapaFotos.Revelar;
                }
                else if (Ok(dtEscolha))
                {
                    proxima = EtapaFotos.Tratamento;
                }
                else
                {
                    proxima = EtapaFotos.Tratamento; // início do fluxo
                }

                // 2) EtapaAtual (o que exibir): próxima pendente, ou “Concluído”
                string etapaAtual =
                    Ok(dtEntrega)
                        ? "Concluído"
                        : proxima switch
                        {
                            EtapaFotos.Tratamento => "Tratamento",
                            EtapaFotos.Revelar => "Revelar",
                            EtapaFotos.Entrega => "Entrega",
                            _ => "Tratamento"
                        };

                // status baseado no prazo da PRÓXIMA etapa
                EtapaStatus st;
                if (Ok(dtEntrega))
                {
                    st = EtapaStatus.Concluido;
                }
                else
                {
                    DateTime prazoProx = proxima switch
                    {
                        EtapaFotos.Tratamento => prazoTrat,
                        EtapaFotos.Revelar => prazoRevelar,
                        EtapaFotos.Entrega => prazoEntrega,
                        _ => prazoTrat
                    };

                    st = hoje.Date > prazoProx.Date ? EtapaStatus.Atrasado
                       : hoje.Date == prazoProx.Date ? EtapaStatus.Hoje
                       : EtapaStatus.Pendente;
                }
                string? mesversarioformatado = Formatar.FormatMesversario(a.Mesversario);

                result.Add(new FotoProcessoVM
                {
                    AgendamentoId = a.Id,
                    Data = a.Data,
                    Cliente = a.ClienteNome,
                    Crianca = a.Crianca,
                    Telefone = a.Telefone,
                    Mesversario = a.Mesversario,
                    MesversarioFormatado = mesversarioformatado,
                    Servico = a.Servico,
                    EtapaAtual = etapaAtual,
                    EscolhaData = Ok(dtEscolha) ? dtEscolha : null,
                    TratamentoData = Ok(dtTrat) ? dtTrat : null,
                    RevelarData = Ok(dtRevelar) ? dtRevelar : null,
                    EntregaData = Ok(dtEntrega) ? dtEntrega : null,
                    Status = st
                });
            }

            if (status.HasValue)
                result = result.Where(x => x.Status == status.Value).ToList();

            return result;
        }
        public async Task ReagendarAsync(int agendamentoId, DateTime novaData, TimeSpan? novoHorario)
        {
            await using var ctx = _dbFactory.CreateDbContext();

            var ag = await ctx.Agendamentos
                .FirstOrDefaultAsync(a => a.Id == agendamentoId);

            if (ag is null) return;

            ag.Data = novaData.Date; // mantém o Horário existente
            ag.Horario = novoHorario;
            await ctx.SaveChangesAsync();
        }
        // ========= FINANCEIRO =========

        // Query base (projeção leve, 100% traduzível pelo EF)
        public IQueryable<FinanceiroRow> QueryFinanceiro(
            DateTime inicio, DateTime fim,
            int? servicoId = null,
            StatusAgendamento? status = null,string clienteNome = null)
        {
            var fimInclusivo = fim.Date.AddDays(1).AddTicks(-1);

            var baseQ =
                from a in _db.Agendamentos
                .Include(a => a.Cliente)
                .Include(a => a.Servico)
                .AsNoTracking()
                where a.Data >= inicio && a.Data <= fimInclusivo
                select a;
            if (servicoId.HasValue)
                baseQ = baseQ.Where(a => a.ServicoId == servicoId.Value);
            if (status.HasValue)
                baseQ = baseQ.Where(a => a.Status == status.Value);
            if (!string.IsNullOrWhiteSpace(clienteNome))
            {
                var filtro = clienteNome.Trim().ToLower();
                baseQ = baseQ.Where(a => a.Cliente.Nome.ToLower().Contains(filtro));
            }

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
                 ValorPago = a.Pagamentos
                 .Where(p => p.AgendamentoProdutoId == null)
                 .Sum(p => (decimal?)p.Valor) ?? 0m,
                 Status = a.Status
             };

            return q;
        }

        public async Task<FinanceiroResumo> CalcularKpisAsync(DateTime inicio, DateTime fim,
            int? servicoId = null, int? produtoId = null,
            StatusAgendamento? status = null,
            string? clienteNome = null)
        {
            var tid = Environment.CurrentManagedThreadId;
            System.Diagnostics.Debug.WriteLine($"[{T()}] [SRV {_sid}] CalcularKpis START ctx={_db.CtxId} tid={tid}");
            try
            {
                var fimIncl = fim.Date.AddDays(1).AddTicks(-1);

                // base (filtra por período, serviço e status se fornecidos)
                var baseQ = _db.Agendamentos
                    .Include(a => a.Cliente)
                    .Include(a => a.Servico)
                    .AsNoTracking()
                    .Where(a => a.Data >= inicio && a.Data <= fimIncl);

                if (servicoId.HasValue) baseQ = baseQ.Where(a => a.ServicoId == servicoId.Value);
                if (status.HasValue) baseQ = baseQ.Where(a => a.Status == status.Value);
                if (!string.IsNullOrWhiteSpace(clienteNome))
                {
                    var filtro = clienteNome.Trim().ToLower();
                    baseQ = baseQ.Where(a => a.Cliente.Nome.ToLower().Contains(filtro));
                }

                // agendamentos válidos (exceto cancelados) - materializamos só id/valor/status (projeção simples)
                var agds = await baseQ
                    .Where(a => a.Status != StatusAgendamento.Cancelado)
                    .Select(a => new { a.Id, a.Valor, a.Status })
                    .ToListAsync();

                var agdIds = agds.Select(x => x.Id).ToList();

                // pagamentos de serviço (AgendamentoProdutoId == null) somente para os agendamentos do período
                var pagamentosServicoList = await _db.Pagamentos.AsNoTracking()
                    .Where(p => p.AgendamentoProdutoId == null && agdIds.Contains(p.AgendamentoId))
                    .GroupBy(p => p.AgendamentoId)
                    .Select(g => new { AgendamentoId = g.Key, PagoServico = g.Sum(x => x.Valor) })
                    .ToListAsync();

                var pagamentosServicoDict = pagamentosServicoList
                    .ToDictionary(x => x.AgendamentoId, x => x.PagoServico);

                // Junta em memória: para cada agendamento pega o Pago (0 se não houver)
                var kpiRaw = agds
                    .Select(a => new
                    {
                        Valor = a.Valor, // se a.Valor for nullable no seu modelo, troque por (a.Valor ?? 0m)
                        Pago = pagamentosServicoDict.TryGetValue(a.Id, out var p) ? p : 0m,
                        a.Status
                    })
                    .ToList();

                // Cálculos em memória — sem EF tentando traduzir Math.Min/ternários
                var receita = kpiRaw.Sum(x => x.Valor);
                var recebido = kpiRaw.Sum(x => x.Pago);
                var aberto = kpiRaw.Sum(x => Math.Max(0m, x.Valor - x.Pago));
                var qtd = kpiRaw.Count;

                var concluidos = kpiRaw.Where(x => x.Status == StatusAgendamento.Concluido)
                                       .Select(x => Math.Min(x.Pago, x.Valor));
                var ticketMedio = qtd > 0 ? receita / qtd : 0m;


                var pagamentosPorItemQ =
                    from p in _db.Set<Agendamento.Pagamento>().AsNoTracking()
                    where p.AgendamentoProdutoId != null
                    group p by p.AgendamentoProdutoId!.Value into g
                    select new { AgendamentoProdutoId = g.Key, Pago = g.Sum(x => x.Valor) };
                var validAgdsQuery = baseQ.Where(a => a.Status != StatusAgendamento.Cancelado);

                // 2) Linhas de venda (join AgendamentoProdutos x agendamentos válidos x pagamentos por item)
                var itensQ =
                    from ap in _db.AgendamentoProdutos.AsNoTracking()
                    join a in validAgdsQuery on ap.AgendamentoId equals a.Id
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
                    ReceitaBruta = receita,
                    Recebido = recebido,
                    EmAberto = Math.Max(0, aberto),
                    QtdAgendamentos = qtd,
                    TicketMedio = Math.Round(ticketMedio, 2),

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

        public Task<List<RecebivelDTO>> ListarEmAbertoAsync(DateTime inicio, DateTime fim,
            int? servicoId = null,
            StatusAgendamento? status = null,
            string? clienteNome = null)
        {
            var fimIncl = fim.Date.AddDays(1).AddTicks(-1);

            var baseQ = _db.Agendamentos.AsNoTracking()
                .Where(a => a.Data >= inicio && a.Data <= fimIncl);

            if (servicoId.HasValue) baseQ = baseQ.Where(a => a.ServicoId == servicoId.Value);
            if (status.HasValue) baseQ = baseQ.Where(a => a.Status == status.Value);
            if (!string.IsNullOrWhiteSpace(clienteNome))
            {
                var filtro = clienteNome.Trim().ToLower();
                baseQ = baseQ.Where(a => a.Cliente.Nome.ToLower().Contains(filtro));
            }

            var valid = baseQ.Where(a => a.Status != StatusAgendamento.Cancelado);

            var q = from a in valid
                    join s in _db.Servicos.AsNoTracking() on a.ServicoId equals s.Id into sj
                    from s in sj.DefaultIfEmpty()
                    join c in _db.Clientes.AsNoTracking() on a.ClienteId equals c.Id into cj
                    from c in cj.DefaultIfEmpty()
                    let pago = a.Pagamentos.Where(p => p.Valor != 0)
                       .Sum(p => (decimal?)p.Valor) ?? 0m
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
         DateTime inicio, DateTime fim,
         int? servicoId = null, StatusAgendamento? status = null,
         string? clienteNome = null)
        {
            var fimIncl = fim.Date.AddDays(1).AddTicks(-1);

            var baseQ = _db.Agendamentos.AsNoTracking()
                .Where(a => a.Data >= inicio && a.Data <= fimIncl);

            if (servicoId.HasValue) baseQ = baseQ.Where(a => a.ServicoId == servicoId.Value);
            if (status.HasValue) baseQ = baseQ.Where(a => a.Status == status.Value);
            if (!string.IsNullOrWhiteSpace(clienteNome))
            {
                var filtro = clienteNome.Trim().ToLower();
                baseQ = baseQ.Where(a => a.Cliente.Nome.ToLower().Contains(filtro));
            }

            var valid = baseQ.Where(a => a.Status != StatusAgendamento.Cancelado);

            var query =
                from g in
                    (from a in valid
                     let pago = a.Pagamentos.Where(p => p.Valor != 0)
                       .Sum(p => (decimal?)p.Valor) ?? 0m
                     group new { a, pago } by a.ServicoId into grp
                     select new
                     {
                         ServicoId = grp.Key,
                         Receita = grp.Sum(x => x.pago),
                         Qtd = grp.Count(),
                         TicketMedio = grp.Average(x => x.pago)
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
     DateTime inicio, DateTime fim, int? produtoId = null, StatusAgendamento? status = null, string? clienteNome = null)
        {
            var fimIncl = fim.Date.AddDays(1).AddTicks(-1);

            // Agendamentos válidos no intervalo (exceto cancelados e com filtro de status, se houver)
            var agdsValid = _db.Agendamentos
                        .Include(a => a.Cliente)
                        .Include(a => a.Servico)
                        .AsNoTracking()
                        .Where(a => a.Data >= inicio && a.Data <= fimIncl)
                        .Where(a => a.Status != StatusAgendamento.Cancelado); 

            if (status.HasValue)
                agdsValid = agdsValid.Where(a => a.Status == status.Value);
            if (!string.IsNullOrWhiteSpace(clienteNome))
            {
                var filtro = clienteNome.Trim().ToLower();
                agdsValid = agdsValid.Where(a => a.Cliente.Nome.ToLower().Contains(filtro));
            }

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
