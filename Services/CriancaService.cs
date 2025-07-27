using System;
using System.Collections.Generic;
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
            var criancasComNascimento = _db.Criancas
                .Where(c => c.Nascimento != null)
                .ToList();

            var hoje = DateOnly.FromDateTime(DateTime.Today);
            var aniversariosDoMes = new StringBuilder();
            var aniversariantes = criancasComNascimento
            .Where(c => c.Nascimento.HasValue && c.Nascimento.Value.Day == hoje.Day)
            .ToList();

            foreach (var crianca in criancasComNascimento)
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
                if (aniversariantes.Any())
                {
                    if (Properties.Settings.Default.UltimoAvisoDia != hoje.Day ||
                        Properties.Settings.Default.UltimoAvisoMes != hoje.Month ||
                        Properties.Settings.Default.UltimoAvisoAno != hoje.Year)
                    {
                        var sb = new StringBuilder();

                        foreach (var c in aniversariantes)
                        {
                            sb.AppendLine($"🎉 {c.Nome} ({c.Cliente?.Nome}) faz mesversário hoje ({nascimento:dd/MM})!");
                        }

                        MessageBox.Show(sb.ToString(), "Mesversários de Hoje", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Salva para não mostrar de novo hoje
                        Properties.Settings.Default.UltimoAvisoDia = hoje.Day;
                        Properties.Settings.Default.UltimoAvisoMes = hoje.Month;
                        Properties.Settings.Default.UltimoAvisoAno = hoje.Year;
                        Properties.Settings.Default.Save();
                    }



                    if (nascimento.Month == hoje.Month)
                    {
                        var cliente = _db.Clientes.FirstOrDefault(c => c.Id == crianca.ClienteId);
                        var nomeCliente = cliente?.Nome ?? "Desconhecido";
                        aniversariosDoMes.AppendLine($"- {crianca.Nome} (Cliente: {nomeCliente})");
                    }


                    if (aniversariosDoMes.Length > 0)
                    {
                        MessageBox.Show(
                            "Crianças que fazem aniversário este mês:\n\n" + aniversariosDoMes.ToString(),
                            "Aniversários do Mês",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information
                        );
                    }
                }
                _db.SaveChanges();
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
