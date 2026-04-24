using CoreService.Application.Contracts;
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
            Status = @event.Status,
            CreatedBy = @event.CreatedBy,
            ModifiedBy = @event.ModifiedBy,
            UpdatedAt = @event.UpdatedAt,
            CreatedAt = @event.CreatedAt,
            Seen = false,
            AdditionalJsonData = @event.AdditionalJsonData
        };

        await _dbContext.RequestMetaData.AddAsync(requestMetaData, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
