using DOmniBus.Lite;
using EmployeeManagementService.Application.Common;

namespace EmployeeManagementService.Application.MessageEmitters.RequestMetaDataEmitter.Create;

public record CreateRequestMetaDataEvent : MessageBase, IEvent
{
    public required int Id { get; set; }
    public required string RequestType { get; set; }
    public required string Status { get; set; }

    public required string CreatedBy { get; set; }
    public required string ModifiedBy { get; set; }
    public required DateTimeOffset UpdatedAt { get; set; }

    public string? AdditionalJsonData { get; set; }

    public IReadOnlyCollection<ApprovalTargetMessage> ApprovalTargets { get; set; } = [];
}
