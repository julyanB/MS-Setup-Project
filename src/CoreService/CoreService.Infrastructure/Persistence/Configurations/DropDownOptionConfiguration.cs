using CoreService.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreService.Infrastructure.Persistence.Configurations;

public class DropDownOptionConfiguration : IEntityTypeConfiguration<DropDownOption>
{
    public void Configure(EntityTypeBuilder<DropDownOption> builder)
    {
        builder.ToTable("DropDownOptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Flow)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.Label)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.SortOrder)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.MetadataJson)
            .IsRequired(false);

        builder.HasIndex(x => new { x.Flow, x.Key, x.Code })
            .IsUnique()
            .HasDatabaseName("UX_DropDownOptions_Flow_Key_Code");

        builder.HasIndex(x => new { x.Flow, x.Key, x.IsActive, x.SortOrder })
            .HasDatabaseName("IX_DropDownOptions_Flow_Key_Active_Order");
    }
}
