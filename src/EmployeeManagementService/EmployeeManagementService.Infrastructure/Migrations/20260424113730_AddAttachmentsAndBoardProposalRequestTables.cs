using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeManagementService.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddAttachmentsAndBoardProposalRequestTables : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Attachments",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                RequestType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                RequestId = table.Column<int>(type: "int", nullable: false),
                Section = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                SectionEntityId = table.Column<int>(type: "int", nullable: true),
                DocumentType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                DocumentName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                CustomDocumentName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                FileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                ContentType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                FileExtension = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                SizeInBytes = table.Column<long>(type: "bigint", nullable: false),
                Content = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                UploadedByEmployeeId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                ModifiedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Attachments", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "BoardProposalRequests",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                MeetingCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                MeetingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                MeetingType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                MeetingFormat = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                SecretaryEmployeeId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                HeldAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                ModifiedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                Status = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, defaultValue: "Draft")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BoardProposalRequests", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "BoardProposalAgendaItems",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                BoardProposalRequestId = table.Column<int>(type: "int", nullable: false),
                Title = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                InitiatorEmployeeId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                ResponsibleBoardMemberEmployeeId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                PresenterEmployeeId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                Category = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                Description = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: true),
                Order = table.Column<int>(type: "int", nullable: false),
                DecisionStatus = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                DecisionText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                FinalVote = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                ModifiedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BoardProposalAgendaItems", x => x.Id);
                table.ForeignKey(
                    name: "FK_BoardProposalAgendaItems_BoardProposalRequests_BoardProposalRequestId",
                    column: x => x.BoardProposalRequestId,
                    principalTable: "BoardProposalRequests",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "BoardProposalTasks",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                AgendaItemId = table.Column<int>(type: "int", nullable: false),
                Title = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ResponsibleEmployeeId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                Status = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, defaultValue: "ToDo"),
                ExtendedDueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                ModifiedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BoardProposalTasks", x => x.Id);
                table.ForeignKey(
                    name: "FK_BoardProposalTasks_BoardProposalAgendaItems_AgendaItemId",
                    column: x => x.AgendaItemId,
                    principalTable: "BoardProposalAgendaItems",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "BoardProposalVotes",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                AgendaItemId = table.Column<int>(type: "int", nullable: false),
                BoardMemberEmployeeId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                VoteType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                ModifiedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BoardProposalVotes", x => x.Id);
                table.ForeignKey(
                    name: "FK_BoardProposalVotes_BoardProposalAgendaItems_AgendaItemId",
                    column: x => x.AgendaItemId,
                    principalTable: "BoardProposalAgendaItems",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Attachments_Request",
            table: "Attachments",
            columns: new[] { "RequestType", "RequestId" });

        migrationBuilder.CreateIndex(
            name: "IX_Attachments_Request_Section",
            table: "Attachments",
            columns: new[] { "RequestType", "RequestId", "Section", "SectionEntityId" });

        migrationBuilder.CreateIndex(
            name: "IX_BoardProposalAgendaItems_Request_Order",
            table: "BoardProposalAgendaItems",
            columns: new[] { "BoardProposalRequestId", "Order" });

        migrationBuilder.CreateIndex(
            name: "IX_BoardProposalRequests_Status",
            table: "BoardProposalRequests",
            column: "Status");

        migrationBuilder.CreateIndex(
            name: "UX_BoardProposalRequests_MeetingCode",
            table: "BoardProposalRequests",
            column: "MeetingCode",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_BoardProposalTasks_AgendaItemId",
            table: "BoardProposalTasks",
            column: "AgendaItemId");

        migrationBuilder.CreateIndex(
            name: "IX_BoardProposalTasks_Responsible_DueDate",
            table: "BoardProposalTasks",
            columns: new[] { "ResponsibleEmployeeId", "DueDate" });

        migrationBuilder.CreateIndex(
            name: "IX_BoardProposalVotes_AgendaItem_BoardMember",
            table: "BoardProposalVotes",
            columns: new[] { "AgendaItemId", "BoardMemberEmployeeId" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Attachments");

        migrationBuilder.DropTable(
            name: "BoardProposalTasks");

        migrationBuilder.DropTable(
            name: "BoardProposalVotes");

        migrationBuilder.DropTable(
            name: "BoardProposalAgendaItems");

        migrationBuilder.DropTable(
            name: "BoardProposalRequests");
    }
}
