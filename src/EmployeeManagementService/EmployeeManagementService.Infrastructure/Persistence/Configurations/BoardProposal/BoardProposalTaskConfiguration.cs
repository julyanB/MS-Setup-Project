using EmployeeManagementService.Domain.Enums.BoardProposal;
using EmployeeManagementService.Domain.Models.BoardProposal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeManagementService.Infrastructure.Persistence.Configurations.BoardProposal;

public class BoardProposalTaskConfiguration : IEntityTypeConfiguration<BoardProposalTask>
{
    public void Configure(EntityTypeBuilder<BoardProposalTask> builder)
    {
        builder.ToTable("BoardProposalTasks");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(x => x.ResponsibleEmployeeId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(64)
            .HasDefaultValue(BoardProposalTaskStatus.ToDo);

        builder.Property(x => x.Order)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(450);

        builder.Property(x => x.ModifiedBy)
            .HasMaxLength(450);

        builder.HasOne(x => x.AgendaItem)
            .WithMany(x => x.Tasks)
            .HasForeignKey(x => x.AgendaItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.ResponsibleEmployeeId, x.DueDate })
            .HasDatabaseName("IX_BoardProposalTasks_Responsible_DueDate");

        builder.HasIndex(x => new { x.AgendaItemId, x.Order })
            .HasDatabaseName("IX_BoardProposalTasks_AgendaItem_Order");
    }
}
