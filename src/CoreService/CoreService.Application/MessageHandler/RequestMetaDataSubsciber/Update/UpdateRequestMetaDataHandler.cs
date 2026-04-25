using CoreService.Application.Contracts;
using CoreService.Domain.Enums;
using CoreService.Domain.Models;
using DOmniBus.Lite;
using Microsoft.EntityFrameworkCore;

namespace CoreService.Application.MessageHandler.RequestMetaDataSubsciber.Update;

public class UpdateRequestMetaDataHandler : IEventHandler<UpdateRequestMetaDataEvent>
{
    private readonly ICoreServiceDbContext _dbContext;

    public UpdateRequestMetaDataHandler(ICoreServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(UpdateRequestMetaDataEvent @event, CancellationToken cancellationToken)
    {
        await _dbContext.ExecuteUpdateAsync(
            _dbContext.RequestMetaData.Where(r => r.Id == @event.Id && r.RequestType == @event.RequestType), x => x
                .SetProperty(p => p.Status, p => @event.Status ?? p.Status)
                .SetProperty(p => p.CreatedBy, p => @event.CreatedBy ?? p.CreatedBy)
                .SetProperty(p => p.ModifiedBy, p => @event.ModifiedBy ?? p.ModifiedBy)
                .SetProperty(p => p.UpdatedAt, p => @event.UpdatedAt ?? p.UpdatedAt)
                .SetProperty(p => p.Seen, p => false)
                .SetProperty(p => p.AdditionalJsonData, p => @event.AdditionalJsonData ?? p.AdditionalJsonData),
            cancellationToken);

        if (@event.ApprovalTargets is null)
        {
            return;
        }

        await _dbContext.ExecuteDeleteAsync(
            _dbContext.RequestMetaDataApprovalTargets.Where(x =>
                x.RequestId == @event.Id && x.RequestType == @event.RequestType),
            cancellationToken);

        var targets = @event.ApprovalTargets
            .Where(x => !string.IsNullOrWhiteSpace(x.TargetValue))
            .Select(x => new RequestMetaDataApprovalTarget
            {
                RequestId = @event.Id,
                RequestType = @event.RequestType,
                TargetType = Enum.Parse<RequestApprovalTargetType>(x.TargetType, ignoreCase: true),
                TargetValue = x.TargetValue.Trim(),
                Status = Enum.Parse<RequestApprovalAssignmentStatus>(x.Status, ignoreCase: true)
            })
            .ToList();

        if (targets.Count > 0)
        {
            await _dbContext.RequestMetaDataApprovalTargets.AddRangeAsync(targets, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
