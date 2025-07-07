using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgendaNovo.Migrations
{
    /// <inheritdoc />
    public partial class ajustesRelacionamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Criancas_Clientes_ClienteId",
                table: "Criancas");

            migrationBuilder.AddColumn<int>(
                name: "CriancaId1",
                table: "Agendamentos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Agendamentos_CriancaId1",
                table: "Agendamentos",
                column: "CriancaId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Agendamentos_Criancas_CriancaId1",
                table: "Agendamentos",
                column: "CriancaId1",
                principalTable: "Criancas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Criancas_Clientes_ClienteId",
                table: "Criancas",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Agendamentos_Criancas_CriancaId1",
                table: "Agendamentos");

            migrationBuilder.DropForeignKey(
                name: "FK_Criancas_Clientes_ClienteId",
                table: "Criancas");

            migrationBuilder.DropIndex(
                name: "IX_Agendamentos_CriancaId1",
                table: "Agendamentos");

            migrationBuilder.DropColumn(
                name: "CriancaId1",
                table: "Agendamentos");

            migrationBuilder.AddForeignKey(
                name: "FK_Criancas_Clientes_ClienteId",
                table: "Criancas",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
