using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgendaNovo.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaColunaObservacaoEPacotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Observacao",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PacoteId",
                table: "Clientes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Pacote",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Categoria = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pacote", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_PacoteId",
                table: "Clientes",
                column: "PacoteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clientes_Pacote_PacoteId",
                table: "Clientes",
                column: "PacoteId",
                principalTable: "Pacote",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clientes_Pacote_PacoteId",
                table: "Clientes");

            migrationBuilder.DropTable(
                name: "Pacote");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_PacoteId",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Observacao",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "PacoteId",
                table: "Clientes");
        }
    }
}
