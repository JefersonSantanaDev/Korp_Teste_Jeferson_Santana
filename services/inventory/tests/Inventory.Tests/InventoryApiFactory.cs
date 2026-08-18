using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;

namespace Inventory.Tests;

/// <summary>
/// Hosts the real Inventory API against a real, disposable PostgreSQL container
/// (Testcontainers) — per AGENTS.md, EF InMemory cannot prove PostgreSQL-specific
/// constraints, atomic conditional updates, or concurrent-row locking.
/// </summary>
public sealed class InventoryApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer postgres = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("inventory_test_db")
        .WithUsername("korp_test")
        .WithPassword("korp_test")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:InventoryDatabase"] = postgres.GetConnectionString(),
            });
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
