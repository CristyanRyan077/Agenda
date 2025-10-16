using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgendaNovo.Migrations
{
    /// <inheritdoc />
    public partial class tabelaagendamentoetapas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
        name: "AgendamentoEtapas",
        columns: table => new
        {
            Id = table.Column<int>(nullable: false)
                .Annotation("SqlServer:Identity", "1, 1"),
            AgendamentoId = table.Column<int>(nullable: false),
            Etapa = table.Column<int>(nullable: false),
            DataConclusao = table.Column<DateTime>(type: "datetime2", nullable: false),
            Observacao = table.Column<string>(type: "nvarchar(1000)", nullable: true),
            CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
            UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
        },
        constraints: table =>
        {
            table.PrimaryKey("PK_AgendamentoEtapas", x => x.Id);
            table.ForeignKey(
                name: "FK_AgendamentoEtapas_Agendamentos_AgendamentoId",
                column: x => x.AgendamentoId,
                principalTable: "Agendamentos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        });

            migrationBuilder.CreateIndex(
                name: "IX_AgendamentoEtapas_AgendamentoId_Etapa",
                table: "AgendamentoEtapas",
                columns: new[] { "AgendamentoId", "Etapa" },
                unique: true);

            // Exemplo de drops que "sumiram"
            migrationBuilder.DropColumn(name: "EntregueEm", table: "Agendamentos");
            migrationBuilder.DropColumn(name: "EscolhaFeitaEm", table: "Agendamentos");
            migrationBuilder.DropColumn(name: "PrazoTratarDias", table: "Agendamentos");
            migrationBuilder.DropColumn(name: "TratadasEm", table: "Agendamentos");
            migrationBuilder.DropColumn(name: "ProducaoConcluidaEm", table: "Agendamentos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(name: "EntregueEm", table: "Agendamentos", type: "datetime2", nullable: true);
            migrationBuilder.AddColumn<int>(name: "EscolhaFeitaEm", table: "Agendamentos", type: "datetime2", nullable: true);
            migrationBuilder.AddColumn<string>(name: "TratadasEm", table: "Agendamentos", type: "datetime2", nullable: true);
            migrationBuilder.AddColumn<int>(name: "PrazoTratarDias", table: "Agendamentos", type: "int", nullable: false);
            migrationBuilder.AddColumn<string>(name: "ProducaoConcluidaEm", table: "Agendamentos", type: "datetime2", nullable: true);

            migrationBuilder.DropTable(name: "AgendamentoEtapas");
        }
    }
}
