using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreService.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddEmplApproval : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "RequestMetaDataApprovalTargets",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                RequestType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                RequestId = table.Column<int>(type: "int", nullable: false),
                TargetType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                TargetValue = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false, defaultValue: "Active")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RequestMetaDataApprovalTargets", x => x.Id);
                table.ForeignKey(
                    name: "FK_RequestMetaDataApprovalTargets_RequestMetaData_RequestType_RequestId",
                    columns: x => new { x.RequestType, x.RequestId },
                    principalTable: "RequestMetaData",
                    principalColumns: new[] { "RequestType", "Id" },
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_RequestMetaDataApprovalTargets_Request",
            table: "RequestMetaDataApprovalTargets",
            columns: new[] { "RequestType", "RequestId" });

        migrationBuilder.CreateIndex(
            name: "IX_RequestMetaDataApprovalTargets_Target_Status",
            table: "RequestMetaDataApprovalTargets",
            columns: new[] { "TargetType", "TargetValue", "Status" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "RequestMetaDataApprovalTargets");
    }
}
