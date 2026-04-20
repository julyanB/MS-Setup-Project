using EmployeeManagementService.Domain.Common;
using EmployeeManagementService.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeManagementService.Infrastructure.Persistence.Extensions;

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
