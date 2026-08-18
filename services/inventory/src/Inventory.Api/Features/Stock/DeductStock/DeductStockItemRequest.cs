using System.ComponentModel.DataAnnotations;

namespace Inventory.Api.Features.Stock.DeductStock;

public sealed record DeductStockItemRequest
{
    [Required]
    public Guid ProductId { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
    public int Quantity { get; init; }
}
