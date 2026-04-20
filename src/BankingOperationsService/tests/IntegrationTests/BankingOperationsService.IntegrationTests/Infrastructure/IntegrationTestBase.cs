using BankingOperationsService.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Refit;

namespace BankingOperationsService.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase
{
    private BankingOperationsServiceWebAppFactory _factory = null!;
    private IServiceScope _scope = null!;

    protected HttpClient Client { get; private set; } = null!;

    protected BankingOperationsServiceDbContext DbContext { get; private set; } = null!;

    [OneTimeSetUp]
    public async Task BaseOneTimeSetUp()
    {
        _factory = new BankingOperationsServiceWebAppFactory();
        await _factory.StartContainerAsync();
        Client = _factory.CreateClient();
        await _factory.MigrateDatabaseAsync();
        _scope = _factory.Services.CreateScope();
        DbContext = _scope.ServiceProvider.GetRequiredService<BankingOperationsServiceDbContext>();
    }

    [OneTimeTearDown]
    public async Task BaseOneTimeTearDown()
    {
        _scope.Dispose();
        Client.Dispose();
        await _factory.DisposeAsync();
    }

    protected T Api<T>() => RestService.For<T>(Client);
}
