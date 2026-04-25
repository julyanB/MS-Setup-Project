namespace CoreService.Application.Features.RequestMetaData.SearchRequestMetaData;

public record RequestMetaDataItem
{
    public required int Id { get; init; }
    public required string VId { get; init; }
    public required string RequestType { get; init; }
    public required string Status { get; init; }
    public required string CreatedBy { get; init; }
    public required string ModifiedBy { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset UpdatedAt { get; init; }
    public required bool Seen { get; init; }
    public string? AdditionalJsonData { get; init; }
}

public record SearchRequestMetaDataResponse
{
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int TotalCount { get; init; }
    public required IReadOnlyList<RequestMetaDataItem> Items { get; init; }
}
