using System.Text.Json;
using BankingOperationsService.Domain.Common;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BankingOperationsService.Infrastructure.Persistence.Converters;

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
