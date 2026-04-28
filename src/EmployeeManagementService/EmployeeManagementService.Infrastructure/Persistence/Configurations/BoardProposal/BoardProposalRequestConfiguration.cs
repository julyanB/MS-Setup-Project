using EmployeeManagementService.Domain.Enums.BoardProposal;
using EmployeeManagementService.Domain.Models.BoardProposal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeManagementService.Infrastructure.Persistence.Configurations.BoardProposal;

public class BoardProposalRequestConfiguration : IEntityTypeConfiguration<BoardProposalRequest>
{
    public void Configure(EntityTypeBuilder<BoardProposalRequest> builder)
    {
        builder.ToTable("BoardProposalRequests");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.MeetingCode)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(x => x.MeetingDate)
            .IsRequired();

        builder.Property(x => x.MeetingType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.MeetingFormat)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.SecretaryEmployeeId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(64)
            .HasDefaultValue(BoardProposalStatus.Draft);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(450);

        builder.Property(x => x.ModifiedBy)
            .HasMaxLength(450);

        builder.HasIndex(x => x.MeetingCode)
            .IsUnique()
            .HasDatabaseName("UX_BoardProposalRequests_MeetingCode");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("IX_BoardProposalRequests_Status");

        builder.HasMany(x => x.ApprovalAssignments)
            .WithOne()
            .HasForeignKey(x => x.RequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }

}
