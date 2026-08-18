using System.ComponentModel.DataAnnotations;

namespace Inventory.Api.Features.Stock.DeductStock;

public sealed record DeductStockRequest
{
    [Required]
    public Guid OperationId { get; init; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one item is required.")]
    public List<DeductStockItemRequest> Items { get; init; } = [];
}
