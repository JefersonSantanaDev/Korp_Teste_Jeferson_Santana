using System.Net;
using System.Net.Http.Json;
using Billing.Api.Clients.Inventory.Contracts;
using Billing.Api.Common.Errors;
using Billing.Api.Features.Invoices;
using Billing.Api.Features.Invoices.CreateInvoice;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NSubstitute.ExceptionExtensions;

namespace Billing.Tests;

[Collection(nameof(BillingTestCollection))]
public sealed class InvoiceCloseTests(BillingApiFactory factory) : IAsyncLifetime
{
    public Task InitializeAsync()
    {
        factory.InventoryClient.ClearSubstitute();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<InvoiceResponse> CreateInvoiceAsync(HttpClient client, Guid productId, int quantity = 1)
    {
        factory.InventoryClient
            .GetProductsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([new InventoryProductResponse(productId, "PROD-CLOSE", "Produto", 10)]);

        var request = new CreateInvoiceRequest
        {
            Items = [new CreateInvoiceItemRequest { ProductId = productId, Quantity = quantity }],
        };

        var response = await client.PostAsJsonAsync("/api/invoices", request);
        return (await response.Content.ReadFromJsonAsync<InvoiceResponse>())!;
    }

    [Fact]
    public async Task CloseInvoice_WhenOpen_ClosesSuccessfully()
    {
        var client = factory.CreateClient();
        var invoice = await CreateInvoiceAsync(client, Guid.NewGuid());

        factory.InventoryClient
            .DeductStockAsync(invoice.Id, Arg.Any<IReadOnlyList<StockDeductionItem>>(), Arg.Any<CancellationToken>())
            .Returns("processed");

        var response = await client.PostAsync($"/api/invoices/{invoice.Id}/close", content: null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var closed = await response.Content.ReadFromJsonAsync<InvoiceResponse>();
        Assert.Equal("Closed", closed!.Status);
        Assert.NotNull(closed.ClosedAt);
    }

    [Fact]
    public async Task CloseInvoice_WhenAlreadyClosed_ReturnsConflict()
    {
        var client = factory.CreateClient();
        var invoice = await CreateInvoiceAsync(client, Guid.NewGuid());

        factory.InventoryClient
            .DeductStockAsync(invoice.Id, Arg.Any<IReadOnlyList<StockDeductionItem>>(), Arg.Any<CancellationToken>())
            .Returns("processed");

        var first = await client.PostAsync($"/api/invoices/{invoice.Id}/close", content: null);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        var second = await client.PostAsync($"/api/invoices/{invoice.Id}/close", content: null);

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task CloseInvoice_WhenInventoryUnavailable_LeavesInvoiceOpen()
    {
        var client = factory.CreateClient();
        var invoice = await CreateInvoiceAsync(client, Guid.NewGuid());

        factory.InventoryClient
            .DeductStockAsync(invoice.Id, Arg.Any<IReadOnlyList<StockDeductionItem>>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InventoryUnavailableException("Inventory is down."));

        var response = await client.PostAsync($"/api/invoices/{invoice.Id}/close", content: null);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);

        var reloaded = await (await client.GetAsync($"/api/invoices/{invoice.Id}")).Content.ReadFromJsonAsync<InvoiceResponse>();
        Assert.Equal("Open", reloaded!.Status);
    }

    [Fact]
    public async Task CloseInvoice_WhenInventorySucceeds_ClosesInvoiceUsingInvoiceIdAsOperationId()
    {
        var client = factory.CreateClient();
        var invoice = await CreateInvoiceAsync(client, Guid.NewGuid());

        factory.InventoryClient
            .DeductStockAsync(invoice.Id, Arg.Any<IReadOnlyList<StockDeductionItem>>(), Arg.Any<CancellationToken>())
            .Returns("processed");

        var response = await client.PostAsync($"/api/invoices/{invoice.Id}/close", content: null);
        var closed = await response.Content.ReadFromJsonAsync<InvoiceResponse>();

        Assert.Equal("Closed", closed!.Status);

        // Verifies the operationId = invoiceId contract: the substitute is only
        // configured to answer for invoice.Id, so a mismatch here would leave the
        // call unconfigured and this assertion would fail.
        _ = factory.InventoryClient.Received(1).DeductStockAsync(
            invoice.Id,
            Arg.Any<IReadOnlyList<StockDeductionItem>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CloseInvoice_RetryAfterAlreadyProcessed_ConvergesWithoutTreatingItAsFailure()
    {
        // Simulates Inventory having already processed the deduction (e.g. a prior
        // close attempt's response was lost) — Billing must still close successfully
        // on retry instead of treating "already_processed" as an error.
        var client = factory.CreateClient();
        var invoice = await CreateInvoiceAsync(client, Guid.NewGuid());

        factory.InventoryClient
            .DeductStockAsync(invoice.Id, Arg.Any<IReadOnlyList<StockDeductionItem>>(), Arg.Any<CancellationToken>())
            .Returns("already_processed");

        var response = await client.PostAsync($"/api/invoices/{invoice.Id}/close", content: null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var closed = await response.Content.ReadFromJsonAsync<InvoiceResponse>();
        Assert.Equal("Closed", closed!.Status);
    }
}
