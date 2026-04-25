using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreService.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddDropDowns : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "DropDownOptions",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Flow = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                Key = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                Code = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                Label = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                SortOrder = table.Column<int>(type: "int", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                MetadataJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DropDownOptions", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_DropDownOptions_Flow_Key_Active_Order",
            table: "DropDownOptions",
            columns: new[] { "Flow", "Key", "IsActive", "SortOrder" });

        migrationBuilder.CreateIndex(
            name: "UX_DropDownOptions_Flow_Key_Code",
            table: "DropDownOptions",
            columns: new[] { "Flow", "Key", "Code" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "DropDownOptions");
    }
}
