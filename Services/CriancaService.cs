using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AgendaNovo.Interfaces;
using AgendaNovo.Models;
using Microsoft.EntityFrameworkCore;

namespace AgendaNovo.Services
{
    public class CriancaService : ICriancaService
    {
        private readonly AgendaContext _db;
        public CriancaService(AgendaContext db)
        {
            _db = db;
        }
          public void AtualizarIdadeDeTodasCriancas()
          {
            Debug.WriteLine("AtualizarIdadeDeTodasCriancas() FOI CHAMADO");
            var hoje = DateOnly.FromDateTime(DateTime.Today);

            var criancas = _db.Criancas
                .Include(c => c.Cliente)
                .ToList();



            foreach (var crianca in criancas)
            {
                var precisaAtualizar = crianca.UltimaAtualizacaoIdade == null ||
                    crianca.UltimaAtualizacaoIdade.Value.Year != hoje.Year ||
                    crianca.UltimaAtualizacaoIdade.Value.Month != hoje.Month;
                if (!precisaAtualizar)
                    continue;

                if (crianca.Nascimento == null)
                {
                    // Apenas marca como atualizado no mês, mesmo sem calcular idade
                    crianca.UltimaAtualizacaoIdade = hoje.ToDateTime(TimeOnly.MinValue);
                    Debug.WriteLine($"[SemNascimento] Marcado como atualizado: {crianca.Nome}");
                    continue;
                }

                AtualizarIdade(crianca, hoje);

            }

            _db.SaveChanges();         
        }
        public void AtualizarIdade(Crianca crianca, DateOnly hoje)
        {
            Debug.WriteLine($"AtualizarIdade chamado para {crianca.Nome} em {DateTime.Now} - DataNascimento: {crianca.Nascimento} - ÚltimaAtualizacao: {crianca.UltimaAtualizacaoIdade}");

            var nascimento = crianca.Nascimento!.Value;
            var anos = hoje.Year - nascimento.Year;
            if (nascimento > hoje.AddYears(-anos)) anos--;
            if (crianca.Idade == anos && crianca.IdadeUnidade == IdadeUnidade.Ano)
                return;
            if (anos == 1)
            {
                crianca.Idade = anos;
                crianca.IdadeUnidade = IdadeUnidade.Ano;
            }

            if (anos > 1)
            {
                crianca.Idade = anos;
                crianca.IdadeUnidade = IdadeUnidade.Anos;
            }
            else
            {
                var meses = (hoje.Year - nascimento.Year) * 12 + hoje.Month - nascimento.Month;
                if (nascimento.Day > hoje.Day) meses--;
                crianca.Idade = meses;
                crianca.IdadeUnidade = IdadeUnidade.Meses;
            }
            crianca.UltimaAtualizacaoIdade = hoje.ToDateTime(TimeOnly.MinValue);
        }
        public void AtualizarIdadeSemNascimento(Crianca crianca, DateOnly hoje)
        {
            Debug.WriteLine($"AtualizarIdade chamado para {crianca.Nome} em {DateTime.Now} - DataNascimento: {crianca.Nascimento} - ÚltimaAtualizacao: {crianca.UltimaAtualizacaoIdade}");

            if (crianca.UltimaAtualizacaoIdade.HasValue &&
               crianca.UltimaAtualizacaoIdade.Value.Month == hoje.Month &&
               crianca.UltimaAtualizacaoIdade.Value.Year == hoje.Year)
            {
                return; // Já atualizou esse mês
            }
            if (crianca.IdadeUnidade == IdadeUnidade.Meses || crianca.IdadeUnidade == IdadeUnidade.Mês)
            {
                crianca.Idade++;
                if (crianca.Idade >= 12)
                {
                    crianca.Idade = 1;
                    crianca.IdadeUnidade = IdadeUnidade.Ano;
                }
            }
            else
            {
                crianca.Idade++;
            }
            crianca.UltimaAtualizacaoIdade = hoje.ToDateTime(TimeOnly.MinValue);
        }

        public List<Crianca> GetByClienteId(int clienteId)
        {
            return _db.Criancas
                .Where(c => c.ClienteId == clienteId)
                .AsNoTracking()
                .ToList();
        }

        public Crianca? GetById(int id)
        {
            return _db.Criancas.Find(id);
        }

        public Crianca AddOrUpdate(Crianca crianca)
        {
            if (crianca.Id == 0)
                _db.Criancas.Add(crianca);
            else
                _db.Criancas.Update(crianca);

            _db.SaveChanges();
            return crianca;
        }

        public void Delete(int id)
        {
            var crianca = _db.Criancas.Find(id);
            if (crianca == null) return;
            _db.Criancas.Remove(crianca);
            _db.SaveChanges();

        }
        public List<Agendamento> GetAgendamentos(int criancaId)
        {
            return _db.Agendamentos
                .Include(a => a.Cliente)
                .Include(a => a.Crianca)
                .Where(a => a.CriancaId == criancaId)
                .AsNoTracking()
                .ToList();
        }
    }

}
