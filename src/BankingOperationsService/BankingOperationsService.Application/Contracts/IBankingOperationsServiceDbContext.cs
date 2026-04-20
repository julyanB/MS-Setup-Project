using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;

namespace BankingOperationsService.Application.Contracts;

public interface IBankingOperationsServiceDbContext
{
    // Add your DbSets here as the domain grows:
    // DbSet<YourEntity> YourEntities { get; }

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
