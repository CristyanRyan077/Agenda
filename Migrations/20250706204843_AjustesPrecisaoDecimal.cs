using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgendaNovo.Migrations
{
    /// <inheritdoc />
    public partial class AjustesPrecisaoDecimal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Agendamentos_Cliente_ClienteId",
                table: "Agendamentos");

            migrationBuilder.DropForeignKey(
                name: "FK_Agendamentos_Crianca_CriancaId",
                table: "Agendamentos");

            migrationBuilder.DropForeignKey(
                name: "FK_Crianca_Cliente_ClienteId",
                table: "Crianca");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Crianca",
                table: "Crianca");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Cliente",
                table: "Cliente");

            migrationBuilder.RenameTable(
                name: "Crianca",
                newName: "Criancas");

            migrationBuilder.RenameTable(
                name: "Cliente",
                newName: "Clientes");

            migrationBuilder.RenameIndex(
                name: "IX_Crianca_ClienteId",
                table: "Criancas",
                newName: "IX_Criancas_ClienteId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Criancas",
                table: "Criancas",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Clientes",
                table: "Clientes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Agendamentos_Clientes_ClienteId",
                table: "Agendamentos",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Agendamentos_Criancas_CriancaId",
                table: "Agendamentos",
                column: "CriancaId",
                principalTable: "Criancas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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
                name: "FK_Agendamentos_Clientes_ClienteId",
                table: "Agendamentos");

            migrationBuilder.DropForeignKey(
                name: "FK_Agendamentos_Criancas_CriancaId",
                table: "Agendamentos");

            migrationBuilder.DropForeignKey(
                name: "FK_Criancas_Clientes_ClienteId",
                table: "Criancas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Criancas",
                table: "Criancas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Clientes",
                table: "Clientes");

            migrationBuilder.RenameTable(
                name: "Criancas",
                newName: "Crianca");

            migrationBuilder.RenameTable(
                name: "Clientes",
                newName: "Cliente");

            migrationBuilder.RenameIndex(
                name: "IX_Criancas_ClienteId",
                table: "Crianca",
                newName: "IX_Crianca_ClienteId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Crianca",
                table: "Crianca",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Cliente",
                table: "Cliente",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Agendamentos_Cliente_ClienteId",
                table: "Agendamentos",
                column: "ClienteId",
                principalTable: "Cliente",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Agendamentos_Crianca_CriancaId",
                table: "Agendamentos",
                column: "CriancaId",
                principalTable: "Crianca",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Crianca_Cliente_ClienteId",
                table: "Crianca",
                column: "ClienteId",
                principalTable: "Cliente",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
