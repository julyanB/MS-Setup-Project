using System.Text.Json;
using CoreService.Domain.Common;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CoreService.Infrastructure.Persistence.Converters;

public class JsonMetadataConverter<T> : ValueConverter<T, string>
    where T : JsonMetadata
{
    public JsonMetadataConverter()
        : base(
            v => v.ToJson(),
            v => JsonSerializer.Deserialize<T>(v, (JsonSerializerOptions?)null)!)
    {
    }
}
