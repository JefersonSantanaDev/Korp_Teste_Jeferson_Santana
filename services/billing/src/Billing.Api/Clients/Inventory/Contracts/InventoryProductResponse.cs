namespace Billing.Api.Clients.Inventory.Contracts;

public sealed record InventoryProductResponse(
    Guid Id,
    string Code,
    string Description,
    int Stock);
