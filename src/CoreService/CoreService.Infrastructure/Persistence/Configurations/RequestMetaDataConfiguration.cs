using CoreService.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreService.Infrastructure.Persistence.Configurations;

public class RequestMetaDataConfiguration : IEntityTypeConfiguration<RequestMetaData>
{
    public void Configure(EntityTypeBuilder<RequestMetaData> builder)
    {
        builder.HasKey(x => new { x.RequestType, x.Id });

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.RequestType)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(x => x.CreatedBy)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.ModifiedBy)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.Property(x => x.AdditionalJsonData)
            .IsRequired(false);
    }
}
