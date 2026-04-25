using EmployeeManagementService.Domain.Models.BoardProposal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeManagementService.Infrastructure.Persistence.Configurations.BoardProposal;

public class BoardProposalAgendaItemConfiguration : IEntityTypeConfiguration<BoardProposalAgendaItem>
{
    public void Configure(EntityTypeBuilder<BoardProposalAgendaItem> builder)
    {
        builder.ToTable("BoardProposalAgendaItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.InitiatorEmployeeId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.ResponsibleBoardMemberEmployeeId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.PresenterEmployeeId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.Category)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(x => x.Description)
            .HasMaxLength(1500);

        builder.Property(x => x.DecisionStatus)
            .HasConversion<string>()
            .HasMaxLength(64);

        builder.Property(x => x.FinalVote)
            .HasMaxLength(512);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(450);

        builder.Property(x => x.ModifiedBy)
            .HasMaxLength(450);

        builder.HasOne(x => x.BoardProposalRequest)
            .WithMany(x => x.AgendaItems)
            .HasForeignKey(x => x.BoardProposalRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.BoardProposalRequestId, x.Order })
            .HasDatabaseName("IX_BoardProposalAgendaItems_Request_Order");
    }
}
