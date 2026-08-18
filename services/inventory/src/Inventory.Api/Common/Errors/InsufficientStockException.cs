namespace Inventory.Api.Common.Errors;

public sealed class InsufficientStockException(Guid productId)
    : Exception($"Insufficient stock for product {productId}.")
{
    public Guid ProductId { get; } = productId;
}
