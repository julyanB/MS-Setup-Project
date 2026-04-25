using DOmniBus.Lite;

namespace CoreService.Application.MessageHandler.RequestMetaDataSubsciber.Update;

public record UpdateRequestMetaDataEvent : MessageBase, IEvent
{
    public required int Id { get; set; }
    public required string RequestType { get; set; }
    public string? Status { get; set; }

    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public string? AdditionalJsonData { get; set; }

    public IReadOnlyCollection<ApprovalTargetMessage>? ApprovalTargets { get; set; }
}

public record ApprovalTargetMessage
{
    public required string TargetType { get; init; }
    public required string TargetValue { get; init; }
    public required string Status { get; init; }
}
