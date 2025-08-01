﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgendaNovo.Models;
using Microsoft.EntityFrameworkCore;

namespace AgendaNovo
{
    public class AgendaContext : DbContext
    {
        public AgendaContext(DbContextOptions<AgendaContext> options)
    : base(options)
        {
        }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Crianca> Criancas { get; set; }
        public DbSet<Agendamento> Agendamentos { get; set; }
        public DbSet <Servico> Servicos { get; set; }
        public DbSet <Pacote> Pacotes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=AgendaDB;");
            optionsBuilder.UseSqlServer("Data Source=PCBRANCOGAMER\\SQLEXPRESS;Initial Catalog=AgendaStudio;Integrated Security=True;Trust Server Certificate=True");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Agendamento>(entity =>
            {

                entity.Property(a => a.Valor).HasColumnType("decimal(18,2)");
                entity.Property(a => a.ValorPago).HasColumnType("decimal(18,2)");
            });
            // Relação entre Agendamento e Cliente
            modelBuilder.Entity<Agendamento>()
                .HasOne(a => a.Cliente)
                .WithMany(c => c.Agendamentos)
                .HasForeignKey(a => a.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relação entre Agendamento e Criança (caso tenha)
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
