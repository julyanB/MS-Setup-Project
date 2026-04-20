using DOmniBus.Lite;

namespace CoreService.Infrastructure.MessageHandler.RequestMetaDataSubsciber;

public record RequestMetaDataEvent : MessageBase, IEvent
{
    public required int Id { get; set; }
    public required string RequestType { get; set; }
    public required string Status { get; set; }

    public required string CreatedBy { get; set; }
    public required string ModifiedBy { get; set; }
    public required DateTimeOffset UpdatedAt { get; set; }

    public string? AdditionalJsonData { get; set; }
}
