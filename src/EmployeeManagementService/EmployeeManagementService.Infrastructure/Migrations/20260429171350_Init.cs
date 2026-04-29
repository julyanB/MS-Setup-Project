using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeManagementService.Infrastructure.Migrations;

/// <inheritdoc />
public partial class Init : Migration
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
                MeetingDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                MeetingType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                MeetingFormat = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                SecretaryEmployeeId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                SentAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                HeldAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                ClosedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
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
            name: "Permissions",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Permissions", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Roles",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Roles", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                IsExternal = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "BoardProposalAgendaItems",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Title = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                Category = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                Description = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: true),
                Order = table.Column<int>(type: "int", nullable: false),
                DecisionStatus = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                DecisionText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                FinalVote = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                InitiatorEmployeeId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                ResponsibleBoardMemberEmployeeId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                PresenterEmployeeId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                BoardProposalRequestId = table.Column<int>(type: "int", nullable: false),
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

        migrationBuilder.CreateTable(
            name: "RolePermissions",
            columns: table => new
            {
                RoleId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                PermissionId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.PermissionId });
                table.ForeignKey(
                    name: "FK_RolePermissions_Permissions_PermissionId",
                    column: x => x.PermissionId,
                    principalTable: "Permissions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_RolePermissions_Roles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "Roles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "UserPermissions",
            columns: table => new
            {
                UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                PermissionId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserPermissions", x => new { x.UserId, x.PermissionId });
                table.ForeignKey(
                    name: "FK_UserPermissions_Permissions_PermissionId",
                    column: x => x.PermissionId,
                    principalTable: "Permissions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_UserPermissions_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "UserRoles",
            columns: table => new
            {
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                table.ForeignKey(
                    name: "FK_UserRoles_Roles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "Roles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_UserRoles_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
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
                DueDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                Order = table.Column<int>(type: "int", nullable: false),
                Status = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, defaultValue: "ToDo"),
                ExtendedDueDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
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
            name: "IX_BoardProposalTasks_AgendaItem_Order",
            table: "BoardProposalTasks",
            columns: new[] { "AgendaItemId", "Order" });

        migrationBuilder.CreateIndex(
            name: "IX_BoardProposalTasks_Responsible_DueDate",
            table: "BoardProposalTasks",
            columns: new[] { "ResponsibleEmployeeId", "DueDate" });

        migrationBuilder.CreateIndex(
            name: "IX_BoardProposalVotes_AgendaItem_BoardMember",
            table: "BoardProposalVotes",
            columns: new[] { "AgendaItemId", "BoardMemberEmployeeId" });

        migrationBuilder.CreateIndex(
            name: "UX_Permissions_Name",
            table: "Permissions",
            column: "Name",
            unique: true);

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

        migrationBuilder.CreateIndex(
            name: "IX_RolePermissions_PermissionId",
            table: "RolePermissions",
            column: "PermissionId");

        migrationBuilder.CreateIndex(
            name: "IX_RolePermissions_RoleId",
            table: "RolePermissions",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "RoleNameIndex",
            table: "Roles",
            column: "NormalizedName",
            unique: true,
            filter: "[NormalizedName] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_UserPermissions_PermissionId",
            table: "UserPermissions",
            column: "PermissionId");

        migrationBuilder.CreateIndex(
            name: "IX_UserPermissions_UserId",
            table: "UserPermissions",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_UserRoles_RoleId",
            table: "UserRoles",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "EmailIndex",
            table: "Users",
            column: "NormalizedEmail");

        migrationBuilder.CreateIndex(
            name: "UserNameIndex",
            table: "Users",
            column: "NormalizedUserName",
            unique: true,
            filter: "[NormalizedUserName] IS NOT NULL");
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
            name: "RequestApprovalAssignments");

        migrationBuilder.DropTable(
            name: "RolePermissions");

        migrationBuilder.DropTable(
            name: "UserPermissions");

        migrationBuilder.DropTable(
            name: "UserRoles");

        migrationBuilder.DropTable(
            name: "BoardProposalAgendaItems");

        migrationBuilder.DropTable(
            name: "Permissions");

        migrationBuilder.DropTable(
            name: "Roles");

        migrationBuilder.DropTable(
            name: "Users");

        migrationBuilder.DropTable(
            name: "BoardProposalRequests");
    }
}
