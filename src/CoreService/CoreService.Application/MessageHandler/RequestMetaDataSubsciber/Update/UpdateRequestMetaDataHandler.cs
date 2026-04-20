using CoreService.Application.Contracts;
using CoreService.Domain.Models;
using DOmniBus.Lite;

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
                .SetProperty(p => p.AdditionalJsonData, p => @event.AdditionalJsonData ?? p.AdditionalJsonData)
            , cancellationToken);
    }
}
