using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeManagementService.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AlterBoardProposalTypesAsEnums : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "FinalVote",
            table: "BoardProposalAgendaItems",
            type: "nvarchar(64)",
            maxLength: 64,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(512)",
            oldMaxLength: 512,
            oldNullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "FinalVote",
            table: "BoardProposalAgendaItems",
            type: "nvarchar(512)",
            maxLength: 512,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(64)",
            oldMaxLength: 64,
            oldNullable: true);
    }
}
