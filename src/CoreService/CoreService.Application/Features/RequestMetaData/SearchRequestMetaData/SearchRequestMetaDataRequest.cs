using Microsoft.AspNetCore.Mvc;

namespace CoreService.Application.Features.RequestMetaData.SearchRequestMetaData;

public record SearchRequestMetaDataRequest
{
    [FromQuery(Name = "page")]
    public int Page { get; init; } = 1;

    [FromQuery(Name = "pageSize")]
    public int PageSize { get; init; } = 20;

    [FromQuery(Name = "requestType")]
    public string? RequestType { get; init; }

    [FromQuery(Name = "status")]
    public string? Status { get; init; }

    [FromQuery(Name = "onlyUnseen")]
    public bool OnlyUnseen { get; init; }

    [FromQuery(Name = "createdBy")]
    public string? CreatedBy { get; init; }
}
