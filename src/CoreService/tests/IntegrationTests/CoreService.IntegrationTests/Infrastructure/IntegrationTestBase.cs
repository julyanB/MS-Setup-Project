using CoreService.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Refit;

namespace CoreService.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase
{
    private CoreServiceWebAppFactory _factory = null!;
    private IServiceScope _scope = null!;

    protected HttpClient Client { get; private set; } = null!;

    protected CoreServiceDbContext DbContext { get; private set; } = null!;

    [OneTimeSetUp]
    public async Task BaseOneTimeSetUp()
    {
        _factory = new CoreServiceWebAppFactory();
        await _factory.StartContainerAsync();
        Client = _factory.CreateClient();
        await _factory.MigrateDatabaseAsync();
        _scope = _factory.Services.CreateScope();
        DbContext = _scope.ServiceProvider.GetRequiredService<CoreServiceDbContext>();
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
