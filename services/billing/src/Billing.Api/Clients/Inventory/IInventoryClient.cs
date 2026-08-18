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
}
