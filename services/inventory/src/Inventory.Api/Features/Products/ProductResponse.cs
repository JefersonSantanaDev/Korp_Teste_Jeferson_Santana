using Inventory.Api.Domain;

namespace Inventory.Api.Features.Products;

public sealed record ProductResponse(
    Guid Id,
    string Code,
    string Description,
    int Stock)
{
    public static ProductResponse FromProduct(Product product) =>
        new(product.Id, product.Code, product.Description, product.Stock);
}
