using Billing.Api.Clients.Inventory;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Testcontainers.PostgreSql;

namespace Billing.Tests;

/// <summary>
/// Hosts the real Billing API against a real, disposable PostgreSQL container
/// (Testcontainers), with <see cref="IInventoryClient"/> replaced by an
/// NSubstitute test double — per the plan, IInventoryClient is exactly the
/// seam meant to be mocked in Billing's tests, since Inventory itself is a
/// separate service with its own test suite.
/// </summary>
public sealed class BillingApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer postgres = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("billing_test_db")
        .WithUsername("korp_test")
        .WithPassword("korp_test")
        .Build();

    public IInventoryClient InventoryClient { get; } = Substitute.For<IInventoryClient>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:BillingDatabase"] = postgres.GetConnectionString(),
                ["Inventory:BaseUrl"] = "http://unused.invalid/",
                ["Inventory:TimeoutSeconds"] = "5",
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IInventoryClient>();
            services.AddSingleton(InventoryClient);
        });
    }

    public async Task InitializeAsync()
    {
        await postgres.StartAsync();

        // Program.cs applies pending migrations on startup; touching Services
        // here builds the host, which runs that startup code before returning.
        _ = Services;
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await postgres.DisposeAsync();
        await base.DisposeAsync();
    }
}
