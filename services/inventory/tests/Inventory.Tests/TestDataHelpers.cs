using System.Net.Http.Json;
using Inventory.Api.Features.Products;
using Inventory.Api.Features.Products.CreateProduct;

namespace Inventory.Tests;

internal static class TestDataHelpers
{
    public static async Task<ProductResponse> CreateProductAsync(HttpClient client, int stock = 10, string? code = null)
    {
        var request = new CreateProductRequest
        {
            Code = code ?? $"TEST-{Guid.NewGuid():N}",
            Description = "Test product",
            Stock = stock,
        };

        var response = await client.PostAsJsonAsync("/api/products", request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ProductResponse>()
            ?? throw new InvalidOperationException("Failed to deserialize product response.");
    }

    public static async Task<ProductResponse> GetProductAsync(HttpClient client, Guid id)
    {
        var response = await client.GetAsync($"/api/products/{id}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ProductResponse>()
            ?? throw new InvalidOperationException("Failed to deserialize product response.");
    }
}
