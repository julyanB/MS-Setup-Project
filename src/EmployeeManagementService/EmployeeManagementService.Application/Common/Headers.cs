using System.Reflection;

namespace EmployeeManagementService.Application.Common;

public static class Headers
{
    public const string CorrelationId = "X-Correlation-Id";
    public const string ChannelName = "X-Channel-Name";
    public const string ServerTime = "X-Server-Time";
    public const string ProcessingDuration = "X-Processing-Duration";

    public static IEnumerable<string> GetAll()
        => typeof(Headers)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => f.IsLiteral && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!);
}
