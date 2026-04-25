using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestMetaDataVId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VId",
                table: "RequestMetaData",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            // Back-fill existing rows so the unique index can be created.
            // VId format: "DCB" + <RequestType enum int> + "0" + <Id>.
            // Currently only BoardProposalRequest (=1) is supported; other
            // unknown request types fall back to the raw type name to keep
            // values traceable rather than colliding on an empty string.
            migrationBuilder.Sql(@"
                UPDATE [RequestMetaData]
                SET [VId] = CASE
                    WHEN [RequestType] = 'BoardProposalRequest'
                        THEN CONCAT('DCB1', '0', CAST([Id] AS NVARCHAR(20)))
                    ELSE CONCAT('DCB', [RequestType], '0', CAST([Id] AS NVARCHAR(20)))
                END
                WHERE [VId] = '';
            ");

            migrationBuilder.CreateIndex(
                name: "IX_RequestMetaData_VId",
                table: "RequestMetaData",
                column: "VId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RequestMetaData_VId",
                table: "RequestMetaData");

            migrationBuilder.DropColumn(
                name: "VId",
                table: "RequestMetaData");
        }
    }
}
