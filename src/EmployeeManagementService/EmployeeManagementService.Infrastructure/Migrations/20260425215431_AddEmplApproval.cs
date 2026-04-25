using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeManagementService.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddEmplApproval : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "RequestApprovalAssignments",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                RequestId = table.Column<int>(type: "int", nullable: false),
                RequestType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                TargetType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                TargetValue = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false, defaultValue: "Active"),
                AssignedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                Comment = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                ModifiedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RequestApprovalAssignments", x => x.Id);
                table.ForeignKey(
                    name: "FK_RequestApprovalAssignments_BoardProposalRequests_RequestId",
                    column: x => x.RequestId,
                    principalTable: "BoardProposalRequests",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_RequestApprovalAssignments_Request_Status",
            table: "RequestApprovalAssignments",
            columns: new[] { "RequestType", "RequestId", "Status" });

        migrationBuilder.CreateIndex(
            name: "IX_RequestApprovalAssignments_RequestId",
            table: "RequestApprovalAssignments",
            column: "RequestId");

        migrationBuilder.CreateIndex(
            name: "IX_RequestApprovalAssignments_Target_Status",
            table: "RequestApprovalAssignments",
            columns: new[] { "TargetType", "TargetValue", "Status" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "RequestApprovalAssignments");
    }
}
