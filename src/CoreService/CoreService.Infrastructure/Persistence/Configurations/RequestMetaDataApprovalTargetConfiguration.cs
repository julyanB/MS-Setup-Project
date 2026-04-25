using CoreService.Domain.Enums;
using CoreService.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreService.Infrastructure.Persistence.Configurations;

public class RequestMetaDataApprovalTargetConfiguration : IEntityTypeConfiguration<RequestMetaDataApprovalTarget>
{
    public void Configure(EntityTypeBuilder<RequestMetaDataApprovalTarget> builder)
    {
        builder.ToTable("RequestMetaDataApprovalTargets");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.RequestType)
            .IsRequired()
            .HasMaxLength(64);

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

        builder.HasOne(x => x.RequestMetaData)
            .WithMany(x => x.ApprovalTargets)
            .HasForeignKey(x => new { x.RequestType, x.RequestId })
            .HasPrincipalKey(x => new { x.RequestType, x.Id })
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.RequestType, x.RequestId })
            .HasDatabaseName("IX_RequestMetaDataApprovalTargets_Request");

        builder.HasIndex(x => new { x.TargetType, x.TargetValue, x.Status })
            .HasDatabaseName("IX_RequestMetaDataApprovalTargets_Target_Status");
    }
}
