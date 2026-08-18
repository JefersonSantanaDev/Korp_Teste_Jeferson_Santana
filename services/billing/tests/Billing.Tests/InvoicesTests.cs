using System.Net;
using System.Net.Http.Json;
using Billing.Api.Clients.Inventory.Contracts;
using Billing.Api.Features.Invoices;
using Billing.Api.Features.Invoices.CreateInvoice;
using NSubstitute;
using NSubstitute.ClearExtensions;

namespace Billing.Tests;

[Collection(nameof(BillingTestCollection))]
public sealed class InvoicesTests(BillingApiFactory factory) : IAsyncLifetime
{
    public Task InitializeAsync()
    {
        factory.InventoryClient.ClearSubstitute();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateInvoice_WithValidItems_StartsOpen()
    {
        var client = factory.CreateClient();
        var productId = Guid.NewGuid();
        factory.InventoryClient
            .GetProductsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([new InventoryProductResponse(productId, "PROD-001", "Teclado Mecânico", 10)]);

        var request = new CreateInvoiceRequest
        {
            Items = [new CreateInvoiceItemRequest { ProductId = productId, Quantity = 2 }],
        };

        var response = await client.PostAsJsonAsync("/api/invoices", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var invoice = await response.Content.ReadFromJsonAsync<InvoiceResponse>();
        Assert.Equal("Open", invoice!.Status);
        Assert.Null(invoice.ClosedAt);
    }

    [Fact]
    public async Task CreateInvoice_GeneratesSequentialNumbers()
    {
        var client = factory.CreateClient();
        var productId = Guid.NewGuid();
        factory.InventoryClient
            .GetProductsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([new InventoryProductResponse(productId, "PROD-002", "Mouse", 10)]);

        var request = new CreateInvoiceRequest
        {
            Items = [new CreateInvoiceItemRequest { ProductId = productId, Quantity = 1 }],
        };

        var first = await (await client.PostAsJsonAsync("/api/invoices", request)).Content.ReadFromJsonAsync<InvoiceResponse>();
        var second = await (await client.PostAsJsonAsync("/api/invoices", request)).Content.ReadFromJsonAsync<InvoiceResponse>();

        // Numbers come from a real Postgres identity column shared across the whole
        // test run, so assert relative order rather than a fixed absolute value.
        Assert.Equal(first!.Number + 1, second!.Number);
    }

    [Fact]
    public async Task CreateInvoice_WithMultipleItems_StoresProductSnapshot()
    {
        var client = factory.CreateClient();
        var productA = Guid.NewGuid();
        var productB = Guid.NewGuid();
        factory.InventoryClient
            .GetProductsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(
            [
                new InventoryProductResponse(productA, "PROD-A", "Produto A", 10),
                new InventoryProductResponse(productB, "PROD-B", "Produto B", 10),
            ]);

        var request = new CreateInvoiceRequest
        {
            Items =
            [
                new CreateInvoiceItemRequest { ProductId = productA, Quantity = 2 },
                new CreateInvoiceItemRequest { ProductId = productB, Quantity = 3 },
            ],
        };

        var response = await client.PostAsJsonAsync("/api/invoices", request);
        var invoice = await response.Content.ReadFromJsonAsync<InvoiceResponse>();

        Assert.Equal(2, invoice!.Items.Count);
        Assert.Contains(invoice.Items, item => item.ProductCode == "PROD-A" && item.Quantity == 2);
        Assert.Contains(invoice.Items, item => item.ProductCode == "PROD-B" && item.Quantity == 3);
    }
}
