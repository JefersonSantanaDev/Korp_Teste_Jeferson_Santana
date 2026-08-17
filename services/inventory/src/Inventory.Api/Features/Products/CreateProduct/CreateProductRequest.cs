using System.ComponentModel.DataAnnotations;
using Inventory.Api.Domain;

namespace Inventory.Api.Features.Products.CreateProduct;

public sealed record CreateProductRequest
{
    [Required]
    [MaxLength(Product.MaxCodeLength)]
    public string Code { get; init; } = string.Empty;

    [Required]
    [MaxLength(Product.MaxDescriptionLength)]
    public string Description { get; init; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int Stock { get; init; }
}
