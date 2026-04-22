using BankingOperationsService.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankingOperationsService.Infrastructure.Persistence.Extensions;

public static class PropertyBuilderCollectionExtensions
{
    public static PropertyBuilder<ICollection<TItem>> HasCollectionConversion<TItem>(
        this PropertyBuilder<ICollection<TItem>> propertyBuilder,
        string columnType = "nvarchar(max)")
    {
        propertyBuilder.HasConversion(new CollectionJsonValueConverter<TItem>());
        propertyBuilder.Metadata.SetValueComparer(new CollectionValueComparer<TItem>());

        return propertyBuilder.HasColumnType(columnType);
    }
}
