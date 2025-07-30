using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgendaNovo.Migrations
{
    /// <inheritdoc />
    public partial class FixPossuiCriancaDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Servicos SET PossuiCrianca = 1 WHERE PossuiCrianca IS NULL");

            // Ajusta coluna para não permitir null e default true
            migrationBuilder.AlterColumn<bool>(
                name: "PossuiCrianca",
                table: "Servicos",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
       name: "PossuiCrianca",
       table: "Servicos",
       type: "bit",
       nullable: true,
       oldClrType: typeof(bool),
       oldType: "bit");
        }
    }
}
