using EmployeeManagementService.Domain.Models.BoardProposal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeManagementService.Infrastructure.Persistence.Configurations.BoardProposal;

public class BoardProposalVoteConfiguration : IEntityTypeConfiguration<BoardProposalVote>
{
    public void Configure(EntityTypeBuilder<BoardProposalVote> builder)
    {
        builder.ToTable("BoardProposalVotes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.BoardMemberEmployeeId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.VoteType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(64);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(450);

        builder.Property(x => x.ModifiedBy)
            .HasMaxLength(450);

        builder.HasOne(x => x.AgendaItem)
            .WithMany(x => x.Votes)
            .HasForeignKey(x => x.AgendaItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.AgendaItemId, x.BoardMemberEmployeeId })
            .HasDatabaseName("IX_BoardProposalVotes_AgendaItem_BoardMember");
    }
}
