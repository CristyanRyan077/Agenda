using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgendaNovo.Migrations
{
    /// <inheritdoc />
    public partial class dadosdepagamentodocliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TotalPagoHistorico",
                table: "Clientes",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPagoMesAtual",
                table: "Clientes",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalPagoHistorico",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "TotalPagoMesAtual",
                table: "Clientes");
        }
    }
}
