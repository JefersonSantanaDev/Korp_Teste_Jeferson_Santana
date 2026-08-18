using System.Net;
using System.Net.Http.Json;
using Inventory.Api.Features.Products;
using Inventory.Api.Features.Products.CreateProduct;

namespace Inventory.Tests;

[Collection(nameof(InventoryTestCollection))]
public sealed class ProductsTests(InventoryApiFactory factory)
{
    [Fact]
    public async Task CreateProduct_WithValidData_ReturnsCreated()
    {
        var client = factory.CreateClient();
        var request = new CreateProductRequest
        {
            Code = $"PROD-{Guid.NewGuid():N}",
            Description = "Teclado Mecânico",
            Stock = 10,
        };

        var response = await client.PostAsJsonAsync("/api/products", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var product = await response.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(product);
        Assert.Equal(request.Code.ToUpperInvariant(), product!.Code);
        Assert.Equal(10, product.Stock);
    }

    [Fact]
    public async Task CreateProduct_WithNegativeStock_ReturnsBadRequest()
    {
        var client = factory.CreateClient();
        var request = new CreateProductRequest
        {
            Code = $"PROD-{Guid.NewGuid():N}",
            Description = "Produto inválido",
            Stock = -1,
        };

        var response = await client.PostAsJsonAsync("/api/products", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_WithDuplicateCode_ReturnsConflict()
    {
        var client = factory.CreateClient();
        var code = $"PROD-{Guid.NewGuid():N}";

        var first = await client.PostAsJsonAsync(
            "/api/products",
            new CreateProductRequest { Code = code, Description = "Original", Stock = 5 });
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await client.PostAsJsonAsync(
            "/api/products",
            new CreateProductRequest { Code = code, Description = "Duplicado", Stock = 1 });

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }
}
