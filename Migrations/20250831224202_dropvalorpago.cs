using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgendaNovo.Migrations
{
    /// <inheritdoc />
    public partial class dropvalorpago : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ValorPagoLegacy",
                table: "Agendamentos");
            migrationBuilder.DropColumn(
    name: "ValorPago",
    table: "Agendamentos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ValorPagoLegacy",
                table: "Agendamentos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
            migrationBuilder.AddColumn<decimal>(
    name: "ValorPago",
    table: "Agendamentos",
    type: "decimal(18,2)",
    nullable: false,
    defaultValue: 0m);

        }
    }
}
