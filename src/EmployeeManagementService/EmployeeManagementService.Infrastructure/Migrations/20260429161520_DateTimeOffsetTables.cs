using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeManagementService.Infrastructure.Migrations;

/// <inheritdoc />
public partial class DateTimeOffsetTables : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "ExtendedDueDate",
            table: "BoardProposalTasks",
            type: "datetimeoffset",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "datetime2",
            oldNullable: true);

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "DueDate",
            table: "BoardProposalTasks",
            type: "datetimeoffset",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "datetime2");

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "SentAt",
            table: "BoardProposalRequests",
            type: "datetimeoffset",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "datetime2",
            oldNullable: true);

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "MeetingDate",
            table: "BoardProposalRequests",
            type: "datetimeoffset",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "datetime2");

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "HeldAt",
            table: "BoardProposalRequests",
            type: "datetimeoffset",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "datetime2",
            oldNullable: true);

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "ClosedAt",
            table: "BoardProposalRequests",
            type: "datetimeoffset",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "datetime2",
            oldNullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<DateTime>(
            name: "ExtendedDueDate",
            table: "BoardProposalTasks",
            type: "datetime2",
            nullable: true,
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetimeoffset",
            oldNullable: true);

        migrationBuilder.AlterColumn<DateTime>(
            name: "DueDate",
            table: "BoardProposalTasks",
            type: "datetime2",
            nullable: false,
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetimeoffset");

        migrationBuilder.AlterColumn<DateTime>(
            name: "SentAt",
            table: "BoardProposalRequests",
            type: "datetime2",
            nullable: true,
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetimeoffset",
            oldNullable: true);

        migrationBuilder.AlterColumn<DateTime>(
            name: "MeetingDate",
            table: "BoardProposalRequests",
            type: "datetime2",
            nullable: false,
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetimeoffset");

        migrationBuilder.AlterColumn<DateTime>(
            name: "HeldAt",
            table: "BoardProposalRequests",
            type: "datetime2",
            nullable: true,
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetimeoffset",
            oldNullable: true);

        migrationBuilder.AlterColumn<DateTime>(
            name: "ClosedAt",
            table: "BoardProposalRequests",
            type: "datetime2",
            nullable: true,
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetimeoffset",
            oldNullable: true);
    }
}
