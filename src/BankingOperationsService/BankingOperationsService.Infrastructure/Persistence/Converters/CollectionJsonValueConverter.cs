using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EmployeeManagementService.Infrastructure.Persistence.Converters;

public sealed class CollectionJsonValueConverter<TItem> : ValueConverter<ICollection<TItem>, string>
{
    public CollectionJsonValueConverter()
        : base(
            collection => JsonSerializer.Serialize(collection, (JsonSerializerOptions?)null),
            json => Deserialize(json))
    {
    }

    private static ICollection<TItem> Deserialize(string json)
        => JsonSerializer.Deserialize<List<TItem>>(json, (JsonSerializerOptions?)null) ?? new List<TItem>();
}
