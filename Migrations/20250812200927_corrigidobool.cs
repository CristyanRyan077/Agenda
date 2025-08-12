using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgendaNovo.Migrations
{
    /// <inheritdoc />
    public partial class corrigidobool : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "possuiAcompanhamentoMensal",
                table: "Pacotes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "possuiAcompanhamentoMensal",
                table: "Pacotes");
        }
    }
}
