using CoreService.Application.Contracts;
using CoreService.Domain.Enums;
using CoreService.Domain.Models;
using DOmniBus.Lite;

namespace CoreService.Application.MessageHandler.RequestMetaDataSubsciber.Create;

public class CreateRequestMetaDataHandler : IEventHandler<CreateRequestMetaDataEvent>
{
    private readonly ICoreServiceDbContext _dbContext;

    public CreateRequestMetaDataHandler(ICoreServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(CreateRequestMetaDataEvent @event, CancellationToken cancellationToken)
    {
        var requestMetaData = new RequestMetaData
        {
            Id = @event.Id,
            RequestType = @event.RequestType,
            VId = RequestTypeExtensions.BuildVId(@event.RequestType, @event.Id),
            Status = @event.Status,
            CreatedBy = @event.CreatedBy,
            ModifiedBy = @event.ModifiedBy,
            UpdatedAt = @event.UpdatedAt,
            CreatedAt = @event.CreatedAt,
            Seen = false,
            AdditionalJsonData = @event.AdditionalJsonData,
            ApprovalTargets = MapTargets(@event.Id, @event.RequestType, @event.ApprovalTargets)
        };

        await _dbContext.RequestMetaData.AddAsync(requestMetaData, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static List<RequestMetaDataApprovalTarget> MapTargets(
        int requestId,
        string requestType,
        IEnumerable<ApprovalTargetMessage> targets)
        => targets
            .Where(x => !string.IsNullOrWhiteSpace(x.TargetValue))
            .Select(x => new RequestMetaDataApprovalTarget
            {
                RequestId = requestId,
                RequestType = requestType,
                TargetType = Enum.Parse<RequestApprovalTargetType>(x.TargetType, ignoreCase: true),
                TargetValue = x.TargetValue.Trim(),
                Status = Enum.Parse<RequestApprovalAssignmentStatus>(x.Status, ignoreCase: true)
            })
            .ToList();
}
