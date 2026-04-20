using CoreService.Domain.Common;
using CoreService.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreService.Infrastructure.Persistence.Extensions;

public static class JsonMetadataExtensions
{
    public static PropertyBuilder<T> HasJsonConversion<T>(this PropertyBuilder<T> propertyBuilder)
        where T : JsonMetadata
    {
        return propertyBuilder
            .HasConversion(new JsonMetadataConverter<T>())
            .HasColumnType("nvarchar(max)");
    }
}
