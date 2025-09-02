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
                name: "FK_Pagamento_Agendamentos_AgendamentoId",
                table: "Pagamento");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Pagamento",
                table: "Pagamento");

            migrationBuilder.RenameTable(
                name: "Pagamento",
                newName: "Pagamentos");

            migrationBuilder.RenameIndex(
                name: "IX_Pagamento_DataPagamento",
                table: "Pagamentos",
                newName: "IX_Pagamentos_DataPagamento");

            migrationBuilder.RenameIndex(
                name: "IX_Pagamento_AgendamentoId",
                table: "Pagamentos",
                newName: "IX_Pagamentos_AgendamentoId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pagamentos",
                table: "Pagamentos",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Pagamentos_Agendamentos_AgendamentoId",
                table: "Pagamentos",
                column: "AgendamentoId",
                principalTable: "Agendamentos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pagamentos_Agendamentos_AgendamentoId",
                table: "Pagamentos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Pagamentos",
                table: "Pagamentos");

            migrationBuilder.RenameTable(
                name: "Pagamentos",
                newName: "Pagamento");

            migrationBuilder.RenameIndex(
                name: "IX_Pagamentos_DataPagamento",
                table: "Pagamento",
                newName: "IX_Pagamento_DataPagamento");

            migrationBuilder.RenameIndex(
                name: "IX_Pagamentos_AgendamentoId",
                table: "Pagamento",
                newName: "IX_Pagamento_AgendamentoId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pagamento",
                table: "Pagamento",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Pagamento_Agendamentos_AgendamentoId",
                table: "Pagamento",
                column: "AgendamentoId",
                principalTable: "Agendamentos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
