using System.Net;
using System.Net.Http.Json;
using Inventory.Api.Features.Stock;
using Inventory.Api.Features.Stock.DeductStock;

namespace Inventory.Tests;

[Collection(nameof(InventoryTestCollection))]
public sealed class StockDeductionsTests(InventoryApiFactory factory)
{
    [Fact]
    public async Task DeductStock_WithSufficientStock_DeductsCorrectly()
    {
        var client = factory.CreateClient();
        var product = await TestDataHelpers.CreateProductAsync(client, stock: 10);

        var request = new DeductStockRequest
        {
            OperationId = Guid.NewGuid(),
            Items = [new DeductStockItemRequest { ProductId = product.Id, Quantity = 3 }],
        };

        var response = await client.PostAsJsonAsync("/api/stock/deductions", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<StockDeductionResponse>();
        Assert.Equal("processed", result!.Status);

        var reloaded = await TestDataHelpers.GetProductAsync(client, product.Id);
        Assert.Equal(7, reloaded.Stock);
    }

    [Fact]
    public async Task DeductStock_WithInsufficientStock_ReturnsConflict()
    {
        var client = factory.CreateClient();
        var product = await TestDataHelpers.CreateProductAsync(client, stock: 2);

        var request = new DeductStockRequest
        {
            OperationId = Guid.NewGuid(),
            Items = [new DeductStockItemRequest { ProductId = product.Id, Quantity = 5 }],
        };

        var response = await client.PostAsJsonAsync("/api/stock/deductions", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var reloaded = await TestDataHelpers.GetProductAsync(client, product.Id);
        Assert.Equal(2, reloaded.Stock);
    }

    [Fact]
    public async Task DeductStock_WithSameOperationId_IsIdempotent()
    {
        var client = factory.CreateClient();
        var product = await TestDataHelpers.CreateProductAsync(client, stock: 10);
        var request = new DeductStockRequest
        {
            OperationId = Guid.NewGuid(),
            Items = [new DeductStockItemRequest { ProductId = product.Id, Quantity = 4 }],
        };

        var first = await client.PostAsJsonAsync("/api/stock/deductions", request);
        var firstResult = await first.Content.ReadFromJsonAsync<StockDeductionResponse>();

        var second = await client.PostAsJsonAsync("/api/stock/deductions", request);
        var secondResult = await second.Content.ReadFromJsonAsync<StockDeductionResponse>();

        Assert.Equal("processed", firstResult!.Status);
        Assert.Equal("already_processed", secondResult!.Status);

        var reloaded = await TestDataHelpers.GetProductAsync(client, product.Id);
        Assert.Equal(6, reloaded.Stock); // deducted once, not twice
    }

    [Fact]
    public async Task DeductStock_WithMultipleItems_IsAtomic()
    {
        var client = factory.CreateClient();
        var sufficientProduct = await TestDataHelpers.CreateProductAsync(client, stock: 10);
        var insufficientProduct = await TestDataHelpers.CreateProductAsync(client, stock: 1);

        var request = new DeductStockRequest
        {
            OperationId = Guid.NewGuid(),
            Items =
            [
                new DeductStockItemRequest { ProductId = sufficientProduct.Id, Quantity = 2 },
                new DeductStockItemRequest { ProductId = insufficientProduct.Id, Quantity = 5 },
            ],
        };

        var response = await client.PostAsJsonAsync("/api/stock/deductions", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        // All-or-nothing: the sufficient item must NOT have been deducted either.
        var reloaded = await TestDataHelpers.GetProductAsync(client, sufficientProduct.Id);
        Assert.Equal(10, reloaded.Stock);
    }

    [Fact]
    public async Task DeductStock_ConcurrentRequestsForLastUnit_OnlyOneSucceeds()
    {
        var client = factory.CreateClient();
        var product = await TestDataHelpers.CreateProductAsync(client, stock: 1);

        var requestA = new DeductStockRequest
        {
            OperationId = Guid.NewGuid(),
            Items = [new DeductStockItemRequest { ProductId = product.Id, Quantity = 1 }],
        };
        var requestB = new DeductStockRequest
        {
            OperationId = Guid.NewGuid(),
            Items = [new DeductStockItemRequest { ProductId = product.Id, Quantity = 1 }],
        };

        var taskA = client.PostAsJsonAsync("/api/stock/deductions", requestA);
        var taskB = client.PostAsJsonAsync("/api/stock/deductions", requestB);
        var responses = await Task.WhenAll(taskA, taskB);

        Assert.Single(responses, r => r.StatusCode == HttpStatusCode.OK);
        Assert.Single(responses, r => r.StatusCode == HttpStatusCode.Conflict);

        var reloaded = await TestDataHelpers.GetProductAsync(client, product.Id);
        Assert.Equal(0, reloaded.Stock);
    }
}
