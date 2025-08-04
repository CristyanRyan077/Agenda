using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgendaNovo.Migrations
{
    /// <inheritdoc />
    public partial class atualizaçãodeidademensalmente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UltimaAtualizacaoIdade",
                table: "Criancas",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UltimaAtualizacaoIdade",
                table: "Criancas");
        }
    }
}
