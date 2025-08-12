using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgendaNovo.Migrations
{
    /// <inheritdoc />
    public partial class boolpossuiacompanhamento_e_numero : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Numero",
                table: "Pacotes",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Numero",
                table: "Pacotes");
        }
    }
}
