using Microsoft.AspNetCore.Mvc;

namespace CoreService.Application.Features.RequestMetaData.MarkRequestMetaDataSeen;

public record MarkRequestMetaDataSeenRequest
{
    [FromRoute]
    public required string RequestType { get; init; }

    [FromRoute]
    public required int Id { get; init; }
}
