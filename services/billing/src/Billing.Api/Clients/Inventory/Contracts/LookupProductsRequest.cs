namespace Billing.Api.Clients.Inventory.Contracts;

public sealed record LookupProductsRequest(IReadOnlyList<Guid> ProductIds);
