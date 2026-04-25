namespace CoreService.Domain.Enums;

public enum RequestType
{
    BoardProposalRequest = 1,
}

public static class RequestTypeExtensions
{
    private const string VIdPrefix = "DCB";

    public static string BuildVId(string requestTypeName, int requestId)
    {
        if (!Enum.TryParse<RequestType>(requestTypeName, ignoreCase: false, out var parsed))
        {
            return $"{VIdPrefix}{requestTypeName}0{requestId}";
        }

        return BuildVId(parsed, requestId);
    }

    public static string BuildVId(this RequestType requestType, int requestId)
        => $"{VIdPrefix}{(int)requestType}0{requestId}";
}
