using System.Reflection;
using CoreService.Application.Contracts;
using CoreService.Application.Exceptions;
using CoreService.Domain.Common;
using CoreService.Domain.Exceptions;
using CoreService.Domain.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;

namespace CoreService.Infrastructure.Persistence;

public class CoreServiceDbContext : DbContext, ICoreServiceDbContext
{
    private const int SqlServerDeadlock = 1205;
    private const int SqlServerUniqueConstraintViolation = 2601;
    private const int SqlServerUniqueIndexViolation = 2627;

    private readonly ICurrentUser? _currentUser;

    public CoreServiceDbContext(DbContextOptions<CoreServiceDbContext> options)
        : base(options)
    {
    }

    public CoreServiceDbContext(
        DbContextOptions<CoreServiceDbContext> options,
        ICurrentUser currentUser)
        : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<RequestMetaData> RequestMetaData { get; set; }
    public DbSet<RequestMetaDataApprovalTarget> RequestMetaDataApprovalTargets { get; set; }

    public DbSet<DropDownOption> DropDownOptions { get; set; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditStamps();
        return await ExecuteWithExceptionHandling(() => base.SaveChangesAsync(cancellationToken));
    }

    public override int SaveChanges()
    {
        ApplyAuditStamps();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAuditStamps();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ApplyAuditStamps();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyAuditStamps()
    {
        var now = DateTimeOffset.UtcNow;
        var userId = _currentUser?.IsAuthenticated == true ? _currentUser.UserId : null;

        foreach (EntityEntry<ITrackable> entry in ChangeTracker.Entries<ITrackable>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Property(nameof(ITrackable.CreatedAt)).IsModified = false;
                    break;
            }
        }

        foreach (EntityEntry<IAuditable> entry in ChangeTracker.Entries<IAuditable>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = userId;
                    entry.Entity.ModifiedBy = userId;
                    break;

                case EntityState.Modified:
                    entry.Entity.ModifiedBy = userId;
                    entry.Property(nameof(IAuditable.CreatedBy)).IsModified = false;
                    break;
            }
        }
    }

    public async Task<int> ExecuteSqlRawAsync(string sql, CancellationToken cancellationToken = default)
        => await ExecuteWithExceptionHandling(() => Database.ExecuteSqlRawAsync(sql, cancellationToken));

    public async Task<int> ExecuteSqlRawAsync(string sql, IEnumerable<object> parameters, CancellationToken cancellationToken = default)
        => await ExecuteWithExceptionHandling(() => Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken));

    public async Task TruncateTable<TEntity>(DbSet<TEntity> dbSet, CancellationToken cancellationToken = default)
        where TEntity : class
    {
        var entityType = Model.FindEntityType(typeof(TEntity));
        var tableName = entityType?.GetSchemaQualifiedTableName();

#pragma warning disable EF1003 // tableName comes from the EF model, not user input
        await ExecuteWithExceptionHandling(() => Database.ExecuteSqlRawAsync(
            "TRUNCATE TABLE " + tableName, cancellationToken));
#pragma warning restore EF1003
    }

    public IEntityType? FindEntityType(Type type)
        => Model.FindEntityType(type);

    public async Task<int> ExecuteUpdateAsync<TEntity>(
        IQueryable<TEntity> query,
        Action<UpdateSettersBuilder<TEntity>> setPropertyCalls,
        CancellationToken cancellationToken = default)
        => await ExecuteWithExceptionHandling(() => query.ExecuteUpdateAsync(setPropertyCalls, cancellationToken));

    public async Task<int> ExecuteDeleteAsync<TEntity>(
        IQueryable<TEntity> query,
        CancellationToken cancellationToken = default)
        => await ExecuteWithExceptionHandling(() => query.ExecuteDeleteAsync(cancellationToken));

    public async Task Reload<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : class
        => await Entry(entity).ReloadAsync(cancellationToken);

    public IQueryable<T> SqlQuery<T>(FormattableString sql)
        => Database.SqlQuery<T>(sql);

    public IQueryable<T> SqlQueryRaw<T>(string sql, params object[] parameters)
        => Database.SqlQueryRaw<T>(sql, parameters);

    public void SetCommandTimeout(TimeSpan timeout)
        => Database.SetCommandTimeout(timeout);

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(builder);
    }

    private static async Task<int> ExecuteWithExceptionHandling(Func<Task<int>> func)
    {
        try
        {
            return await func();
        }
        catch (Exception ex) when (MapToDatabaseException(ex) is { } dbEx)
        {
            throw dbEx;
        }
    }

    // Overload for void-returning operations (e.g. bulk inserts).
#pragma warning disable IDE0051
    private static async Task ExecuteWithExceptionHandling(Func<Task> func)
#pragma warning restore IDE0051
    {
        try
        {
            await func();
        }
        catch (Exception ex) when (MapToDatabaseException(ex) is { } dbEx)
        {
            throw dbEx;
        }
    }

    private static DatabaseException? MapToDatabaseException(Exception ex)
    {
        var sqlException = ex as SqlException
            ?? (ex as DbUpdateException)?.InnerException as SqlException
            ?? (ex as InvalidOperationException)?.InnerException as SqlException;

        if (sqlException is null)
        {
            return null;
        }

        return sqlException.Number switch
        {
            SqlServerDeadlock
                => new DatabaseException(ErrorCodes.ConcurrencyError, sqlException.Message, ex),
            SqlServerUniqueConstraintViolation or SqlServerUniqueIndexViolation
                => new DatabaseException(ErrorCodes.DuplicateKeyError, sqlException.Message, ex),
            _ => null
        };
    }
}
