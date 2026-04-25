using CoreService.Application.Contracts;
using Microsoft.EntityFrameworkCore;

namespace CoreService.Application.Features.RequestMetaData.SearchRequestMetaData;

public class SearchRequestMetaDataRequestHandler
{
    private const int MaxPageSize = 200;

    private readonly ICoreServiceDbContext _dbContext;

    public SearchRequestMetaDataRequestHandler(ICoreServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SearchRequestMetaDataResponse> Handle(
        SearchRequestMetaDataRequest request,
        CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1
            ? 20
            : request.PageSize > MaxPageSize
                ? MaxPageSize
                : request.PageSize;

        var query = _dbContext.RequestMetaData.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.RequestType))
        {
            query = query.Where(x => x.RequestType == request.RequestType);
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(x => x.Status == request.Status);
        }

        if (!string.IsNullOrWhiteSpace(request.CreatedBy))
        {
            query = query.Where(x => x.CreatedBy == request.CreatedBy);
        }

        if (request.OnlyUnseen)
        {
            query = query.Where(x => !x.Seen);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.UpdatedAt)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new RequestMetaDataItem
            {
                Id = x.Id,
                RequestType = x.RequestType,
                Status = x.Status,
                CreatedBy = x.CreatedBy,
                ModifiedBy = x.ModifiedBy,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                Seen = x.Seen,
                AdditionalJsonData = x.AdditionalJsonData,
            })
            .ToListAsync(cancellationToken);

        return new SearchRequestMetaDataResponse
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = items,
        };
    }
}
