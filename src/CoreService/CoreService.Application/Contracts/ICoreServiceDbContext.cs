using CoreService.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;

namespace CoreService.Application.Contracts;

public interface ICoreServiceDbContext
{
    DbSet<RequestMetaData> RequestMetaData { get; set; }
    DbSet<RequestMetaDataApprovalTarget> RequestMetaDataApprovalTargets { get; set; }

    DbSet<DropDownOption> DropDownOptions { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<int> ExecuteSqlRawAsync(string sql, CancellationToken cancellationToken = default);

    Task<int> ExecuteSqlRawAsync(string sql, IEnumerable<object> parameters, CancellationToken cancellationToken = default);

    Task TruncateTable<TEntity>(DbSet<TEntity> dbSet, CancellationToken cancellationToken = default)
        where TEntity : class;

    IEntityType? FindEntityType(Type type);

    Task<int> ExecuteUpdateAsync<TEntity>(
        IQueryable<TEntity> query,
        Action<UpdateSettersBuilder<TEntity>> setPropertyCalls,
        CancellationToken cancellationToken = default);

    Task<int> ExecuteDeleteAsync<TEntity>(
        IQueryable<TEntity> query,
        CancellationToken cancellationToken = default);

    Task Reload<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : class;

    IQueryable<T> SqlQuery<T>(FormattableString sql);

    IQueryable<T> SqlQueryRaw<T>(string sql, params object[] parameters);

    void SetCommandTimeout(TimeSpan timeout);
}
