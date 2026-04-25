using DOmniBus.Lite;
using EmployeeManagementService.Application.Common;

namespace EmployeeManagementService.Application.MessageEmitters.RequestMetaDataEmitter.Update;

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
