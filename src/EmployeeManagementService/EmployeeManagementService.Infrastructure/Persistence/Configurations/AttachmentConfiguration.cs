using EmployeeManagementService.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeManagementService.Infrastructure.Persistence.Configurations;

public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.ToTable("Attachments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.RequestType)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.Section)
            .HasMaxLength(128);

        builder.Property(x => x.DocumentType)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.DocumentName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.CustomDocumentName)
            .HasMaxLength(256);

        builder.Property(x => x.FileName)
            .IsRequired()
            .HasMaxLength(260);

        builder.Property(x => x.ContentType)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.FileExtension)
            .IsRequired()
            .HasMaxLength(16);

        builder.Property(x => x.Content)
            .IsRequired();

        builder.Property(x => x.UploadedByEmployeeId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(450);

        builder.Property(x => x.ModifiedBy)
            .HasMaxLength(450);

        builder.HasIndex(x => new { x.RequestType, x.RequestId })
            .HasDatabaseName("IX_Attachments_Request");

        builder.HasIndex(x => new { x.RequestType, x.RequestId, x.Section, x.SectionEntityId })
            .HasDatabaseName("IX_Attachments_Request_Section");
    }
}
