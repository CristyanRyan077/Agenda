using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgendaNovo.Migrations
{
    /// <inheritdoc />
    public partial class teste : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Agendamentos_Criancas_CriancaId",
                table: "Agendamentos");

            migrationBuilder.AddForeignKey(
                name: "FK_Agendamentos_Criancas_CriancaId",
                table: "Agendamentos",
                column: "CriancaId",
                principalTable: "Criancas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Agendamentos_Criancas_CriancaId",
                table: "Agendamentos");

            migrationBuilder.AddForeignKey(
                name: "FK_Agendamentos_Criancas_CriancaId",
                table: "Agendamentos",
                column: "CriancaId",
                principalTable: "Criancas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
