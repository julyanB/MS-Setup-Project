using CoreService.Application.Contracts;
using CoreService.Domain.Models;
using DOmniBus.Lite;

namespace CoreService.Infrastructure.MessageHandler.RequestMetaDataSubsciber;

public class RequestMetaDataHandler : IEventHandler<RequestMetaDataEvent>
{
    private readonly ICoreServiceDbContext _dbContext;

    public RequestMetaDataHandler(ICoreServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(RequestMetaDataEvent @event, CancellationToken cancellationToken)
    {
        var requestMetaData = new RequestMetaData
        {
            Id = @event.Id,
            RequestType = @event.RequestType,
            Status = @event.Status,
            CreatedBy = @event.CreatedBy,
            ModifiedBy = @event.ModifiedBy,
            UpdatedAt = @event.UpdatedAt,
            CreatedAt = @event.CreatedAt,
            AdditionalJsonData = @event.AdditionalJsonData
        };

        await _dbContext.RequestMetaData.AddAsync(requestMetaData, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
