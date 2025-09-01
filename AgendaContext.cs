using AgendaNovo.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AgendaNovo.Agendamento;

namespace AgendaNovo
{
    public class AgendaContext : DbContext
    {
        public string CtxId { get; } = Guid.NewGuid().ToString("N")[..6];
        public AgendaContext(DbContextOptions<AgendaContext> options)
    : base(options)
        {
            System.Diagnostics.Debug.WriteLine($"[CTX NEW] {CtxId} (thread {Environment.CurrentManagedThreadId})");
        }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Crianca> Criancas { get; set; }
        public DbSet<Agendamento> Agendamentos { get; set; }
        public DbSet <Servico> Servicos { get; set; }
        public DbSet <Pacote> Pacotes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseSqlServer("Data Source=2857AL17;Initial Catalog=AgendaDB;Integrated Security=True;Trust Server Certificate=True;");
            optionsBuilder.UseSqlServer("Data Source=PCBRANCOGAMER\\SQLEXPRESS;Initial Catalog=AgendaStudio;Integrated Security=True;Trust Server Certificate=True");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Pagamento>(e =>
            {
                e.Property(x => x.Valor).HasColumnType("decimal(18,2)");
                e.HasOne(x => x.Agendamento)
                 .WithMany(a => a.Pagamentos)
                 .HasForeignKey(x => x.AgendamentoId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(x => x.AgendamentoId);
                e.HasIndex(x => x.DataPagamento);
            });
            modelBuilder.Entity<Agendamento>(entity =>
            {

                entity.Property(a => a.Valor).HasColumnType("decimal(18,2)");
            });
            // Relação entre Agendamento e Cliente
            modelBuilder.Entity<Agendamento>()
                .HasOne(a => a.Cliente)
                .WithMany(c => c.Agendamentos)
                .HasForeignKey(a => a.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relação entre Agendamento e Criança 
            modelBuilder.Entity<Agendamento>()
                .HasOne(a => a.Crianca)
                .WithMany()
                .HasForeignKey(a => a.CriancaId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Agendamento>()
               .HasOne(a => a.Servico)
               .WithMany()
               .HasForeignKey(a => a.ServicoId)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Agendamento>()
                .HasOne(a => a.Pacote)
                .WithMany()
                .HasForeignKey(a => a.PacoteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relação entre Criança e Cliente (evitando múltiplos caminhos em cascata)
            modelBuilder.Entity<Crianca>()
                .HasOne(c => c.Cliente)
                .WithMany(c => c.Criancas)
                .HasForeignKey(c => c.ClienteId)
                .OnDelete(DeleteBehavior.Cascade); // ou .NoAction()
            modelBuilder.Entity<Servico>()
                .Property(s => s.PossuiCrianca)
                .HasDefaultValue(true);

            base.OnModelCreating(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }
    }
}
