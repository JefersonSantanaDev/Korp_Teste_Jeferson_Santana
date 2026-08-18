using Billing.Api.Clients.Inventory.Contracts;

namespace Billing.Api.Clients.Inventory;

public interface IInventoryClient
{
    /// <summary>
    /// Fetches the current snapshot (code, description, stock) for the given products.
    /// Products that do not exist in Inventory are simply absent from the result.
    /// </summary>
    Task<IReadOnlyList<InventoryProductResponse>> GetProductsAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken);

    /// <summary>
    /// Deducts stock for all items atomically and idempotently. Returns the
    /// Inventory-reported status ("processed" or "already_processed") — both
    /// represent success from the caller's point of view. Throws
    /// <see cref="Common.Errors.InsufficientStockException"/> on a stock conflict
    /// and <see cref="Common.Errors.InventoryUnavailableException"/> when
    /// Inventory cannot be reached or times out.
    /// </summary>
    Task<string> DeductStockAsync(
        Guid operationId,
        IReadOnlyList<StockDeductionItem> items,
        CancellationToken cancellationToken);
}
