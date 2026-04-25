using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeManagementService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBoardProposalTasksOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BoardProposalTasks_AgendaItemId",
                table: "BoardProposalTasks");

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "BoardProposalTasks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_BoardProposalTasks_AgendaItem_Order",
                table: "BoardProposalTasks",
                columns: new[] { "AgendaItemId", "Order" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BoardProposalTasks_AgendaItem_Order",
                table: "BoardProposalTasks");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "BoardProposalTasks");

            migrationBuilder.CreateIndex(
                name: "IX_BoardProposalTasks_AgendaItemId",
                table: "BoardProposalTasks",
                column: "AgendaItemId");
        }
    }
}
