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
            Debug.WriteLine("AtualizarIdadeDeTodasCriancas chamado");
            var criancasComNascimento = _db.Criancas
                .Include(c => c.Cliente)
                .Where(c => c.Nascimento != null)
                .ToList();

            var hoje = DateOnly.FromDateTime(DateTime.Today);

            var aniversariantesDoMes = criancasComNascimento
                .Where(c =>
                    c.Nascimento.Value.Month == hoje.Month
                )
                .ToList();

            // Atualiza idade de todas as crianças
            foreach (var crianca in criancasComNascimento)
                AtualizarIdade(crianca, hoje);

            _db.SaveChanges();

          

            // AVISO DE ANIVERSÁRIO DO MÊS
            if (aniversariantesDoMes.Any() && (Properties.Settings.Default.UltimoAvisoMes == hoje.Month ||
                Properties.Settings.Default.UltimoAvisoAno == hoje.Year))
                
            {
                var sbMes = new StringBuilder();
                foreach (var c in aniversariantesDoMes)
                    sbMes.AppendLine($"- {c.Nome} ({c.Cliente?.Nome})");

                    MessageBox.Show(
                    "Crianças que fazem aniversário este mês:\n\n" + sbMes.ToString(),
                    "Aniversários do Mês",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                //Properties.Settings.Default.Reset();
                Properties.Settings.Default.UltimoAvisoMes = hoje.Month;
                Properties.Settings.Default.UltimoAvisoAno = hoje.Year;
                Properties.Settings.Default.Save();
                Properties.Settings.Default.Save();

            }
            Debug.WriteLine($"Qtd aniversariantes: {aniversariantesDoMes.Count}");
        }
        public void AtualizarIdade(Crianca crianca, DateOnly hoje)
        {
            var nascimento = crianca.Nascimento!.Value;
            var anos = hoje.Year - nascimento.Year;
            if (nascimento > hoje.AddYears(-anos)) anos--;

            if (anos > 0)
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
