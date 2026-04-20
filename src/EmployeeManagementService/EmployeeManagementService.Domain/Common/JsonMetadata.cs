using System.Text.Json;

namespace EmployeeManagementService.Domain.Common;

public abstract class JsonMetadata
{
    public string ToJson()
        => JsonSerializer.Serialize(this, GetType());

    public static T? FromJson<T>(string json) where T : JsonMetadata
        => JsonSerializer.Deserialize<T>(json);

    public override string ToString()
        => ToJson();

    public override bool Equals(object? obj)
    {
        if (obj is null || GetType() != obj.GetType())
        {
            return false;
        }

        return ToJson() == ((JsonMetadata)obj).ToJson();
    }

    public override int GetHashCode()
        => ToJson().GetHashCode();
}
