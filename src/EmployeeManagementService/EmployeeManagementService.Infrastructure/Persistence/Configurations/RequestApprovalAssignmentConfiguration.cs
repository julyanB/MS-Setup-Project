using EmployeeManagementService.Domain.Common;
using EmployeeManagementService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeManagementService.Infrastructure.Persistence.Configurations;

public class RequestApprovalAssignmentConfiguration : IEntityTypeConfiguration<RequestApprovalAssignment<int>>
{
    public void Configure(EntityTypeBuilder<RequestApprovalAssignment<int>> builder)
    {
        builder.ToTable("RequestApprovalAssignments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.RequestType)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.TargetType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.TargetValue)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32)
            .HasDefaultValue(RequestApprovalAssignmentStatus.Active);

        builder.Property(x => x.AssignedAt)
            .IsRequired();

        builder.Property(x => x.Comment)
            .HasMaxLength(1024);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(450);

        builder.Property(x => x.ModifiedBy)
            .HasMaxLength(450);

        builder.HasIndex(x => new { x.RequestType, x.RequestId, x.Status })
            .HasDatabaseName("IX_RequestApprovalAssignments_Request_Status");

        builder.HasIndex(x => new { x.TargetType, x.TargetValue, x.Status })
            .HasDatabaseName("IX_RequestApprovalAssignments_Target_Status");
    }
}
