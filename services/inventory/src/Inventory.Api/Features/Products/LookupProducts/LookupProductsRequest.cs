using System.ComponentModel.DataAnnotations;

namespace Inventory.Api.Features.Products.LookupProducts;

public sealed record LookupProductsRequest
{
    [Required]
    public List<Guid> ProductIds { get; init; } = [];
}
