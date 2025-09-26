using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgendaNovo.Migrations
{
    /// <inheritdoc />
    public partial class etapaprodutos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PrazoTratarDias",
                table: "Servicos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PrazoProducaoDias",
                table: "Produtos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "EntregueEm",
                table: "Agendamentos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EscolhaFeitaEm",
                table: "Agendamentos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PrazoTratarDias",
                table: "Agendamentos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TratadasEm",
                table: "Agendamentos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PrazoProducaoDias",
                table: "AgendamentoProdutos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProducaoConcluidaEm",
                table: "AgendamentoProdutos",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrazoTratarDias",
                table: "Servicos");

            migrationBuilder.DropColumn(
                name: "PrazoProducaoDias",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "EntregueEm",
                table: "Agendamentos");

            migrationBuilder.DropColumn(
                name: "EscolhaFeitaEm",
                table: "Agendamentos");

            migrationBuilder.DropColumn(
                name: "PrazoTratarDias",
                table: "Agendamentos");

            migrationBuilder.DropColumn(
                name: "TratadasEm",
                table: "Agendamentos");

            migrationBuilder.DropColumn(
                name: "PrazoProducaoDias",
                table: "AgendamentoProdutos");

            migrationBuilder.DropColumn(
                name: "ProducaoConcluidaEm",
                table: "AgendamentoProdutos");
        }
    }
}
